using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using TauCode.Data.Text.Exceptions;

namespace TauCode.Data.Text
{
    public readonly struct HostName : IEquatable<HostName>
    {
        #region Constants & Static

        private const int MaxLength = 253;
        private static readonly int MaxIPv6Length = "1111:2222:3333:4444:5555:6666:123.123.123.123".Length;

        private static readonly IdnMapping Idn; // IdnMapping type is thread-safe

        static HostName()
        {
            Idn = new IdnMapping
            {
                UseStd3AsciiRules = true,
            };
        }

        #endregion

        #region Fields

        public readonly HostNameKind Kind;
        public readonly string Value;

        #endregion

        #region ctor

        private HostName(HostNameKind kind, string value)
        {
            this.Kind = kind;
            this.Value = value;
        }

        #endregion

        #region Parsing

        public static HostName Parse(ReadOnlySpan<char> input)
        {
            var parsed = TryParse(input, out var hostName, out var exception);
            if (parsed)
            {
                return hostName;
            }

            throw exception;
        }

        public static bool TryParse(
            ReadOnlySpan<char> input,
            out HostName hostName,
            out TextDataExtractionException exception)
        {
            var consumed = TryExtract(
                input,
                out hostName,
                out exception,
                (span, position) => false);

            return consumed > 0;
        }

        #endregion

        #region Extracting

        // todo ut.
        public static int Extract(
            ReadOnlySpan<char> input,
            out HostName hostName,
            TerminatingDelegate terminatingPredicate = null)
        {
            var consumed = TryExtractInternal(
                input,
                out hostName,
                out var exception,
                terminatingPredicate);

            if (consumed > 0)
            {
                return consumed;
            }

            throw exception;
        }

        public static int TryExtract(
            ReadOnlySpan<char> input,
            out HostName hostName,
            out TextDataExtractionException exception,
            TerminatingDelegate terminatingPredicate = null)
        {
            return TryExtractInternal(
                input,
                out hostName,
                out exception,
                terminatingPredicate);
        }

        #endregion

        #region Private

        public static int TryExtractInternal(
            ReadOnlySpan<char> input,
            out HostName hostName,
            out TextDataExtractionException exception,
            TerminatingDelegate terminatingPredicate = null)
        {
            terminatingPredicate ??= Helper.DefaultTerminatingPredicate;

            if (input.IsEmpty)
            {
                hostName = default;
                exception = Helper.CreateException(ExtractionErrorTag.EmptyInput, null); // todo ut
                return 0;
            }

            var canBeIPv6 = true;
            var periodCount = 0; // period is '.'
            var gotColon = false;
            var nothingButPeriodsAndDigits = true;
            var canBeAscii = true;
            var currentAsciiLabelLength = 0; // see Helper.MaxAsciiLabelLength to find out what label is.

            char? prevChar = null;
            var pos = 0;

            while (true)
            {
                if (
                    pos == input.Length ||
                    (pos == MaxLength && terminatingPredicate(input, pos))
                    )
                {
                    if (
                        prevChar == '.' ||
                        prevChar == '-' ||
                        false
                    )
                    {
                        // these chars cannot be last ones
                        hostName = default;
                        exception = Helper.CreateException(ExtractionErrorTag.UnexpectedEnd, pos);
                        return 0;
                    }

                    break;
                }

                if (pos == MaxLength)
                {
                    hostName = default;
                    exception = Helper.CreateException(ExtractionErrorTag.InputTooLong, pos);
                    return 0;
                }

                if (canBeIPv6 && gotColon && pos == MaxIPv6Length)
                {
                    // got IPv6 here, cannot consume more chars

                    if (terminatingPredicate(input, pos))
                    {
                        // got terminating char, let's try parse IPv6 below
                        break;
                    }
                    else
                    {
                        hostName = default;
                        exception = Helper.CreateException(ExtractionErrorTag.InvalidIPv6Address, 0);
                        return 0;
                    }
                }

                var c = input[pos];

                if (terminatingPredicate(input, pos) ) // todo: ut this
                {
                    if (pos == 0)
                    {
                        hostName = default;
                        exception = Helper.CreateException(ExtractionErrorTag.EmptyInput, null); // note: not '0'
                        return 0;
                    }

                    // got terminating char
                    if (
                        prevChar == '.' ||
                        prevChar == '-' ||
                        false
                    )
                    {
                        // these chars cannot be last ones
                        hostName = default;
                        exception = Helper.CreateException(ExtractionErrorTag.UnexpectedEnd, pos);
                        return 0;
                    }

                    break;
                }

                if (c == '.')
                {
                    var badSituationForPeriod =
                        pos == 0 || // '.' cannot be first char
                        prevChar == '.' || // '.' cannot follow '.'
                        prevChar == '-' || // '.' cannot follow '-'
                        prevChar == ':' || // '.' cannot follow ':'
                        false;

                    if (badSituationForPeriod)
                    {
                        hostName = default;
                        exception = Helper.CreateException(ExtractionErrorTag.UnexpectedChar, pos);
                        return 0;
                    }

                    periodCount++;

                    if (canBeAscii)
                    {
                        currentAsciiLabelLength =
                            0; // '.' cuts off previous label, if there was any. See Helper.MaxAsciiLabelLength to find out what label is.
                    }
                }
                else if (c.IsLatinLetterInternal())
                {
                    var isHexDigit = c.IsHexDigit();

                    nothingButPeriodsAndDigits = false;
                    if (gotColon)
                    {
                        if (isHexDigit)
                        {
                            // ok
                        }
                        else
                        {
                            // got colon, but now have a latin char that is not a hex digit => error.
                            hostName = default;
                            exception = Helper.CreateException(ExtractionErrorTag.UnexpectedChar, pos);
                            return 0;
                        }
                    }

                    if (canBeAscii)
                    {
                        currentAsciiLabelLength++;
                    }

                    if (!isHexDigit)
                    {
                        canBeIPv6 = false;
                    }
                }
                else if (c == ':')
                {
                    if (!canBeIPv6)
                    {
                        exception = Helper.CreateException(ExtractionErrorTag.UnexpectedChar, pos);
                        hostName = default;
                        return 0;
                    }

                    if (periodCount > 0)
                    {
                        // ':' cannot follow a '.'
                        exception = Helper.CreateException(ExtractionErrorTag.UnexpectedChar, pos);
                        hostName = default;
                        return 0;
                    }

                    gotColon = true;
                    nothingButPeriodsAndDigits = false;

                    canBeAscii = false;
                    currentAsciiLabelLength = 0;
                }
                else if (c == '-')
                {
                    var badSituationForHyphen =
                        pos == 0 || // '-' cannot be first char
                        prevChar == '.' || // '-' cannot follow '.'
                        gotColon || // IPv6 address cannot hold '-'
                        false;

                    if (badSituationForHyphen)
                    {
                        hostName = default;
                        exception = Helper.CreateException(ExtractionErrorTag.UnexpectedChar, pos);
                        return 0;
                    }

                    nothingButPeriodsAndDigits = false;

                    if (prevChar == '-')
                    {
                        canBeAscii = false; // two '-' in a row means internationalized domain name
                        currentAsciiLabelLength = 0;
                    }

                    if (canBeAscii)
                    {
                        currentAsciiLabelLength++;
                    }

                    canBeIPv6 = false;
                }
                else if (c.IsUnicodeInternal() || char.IsLetter(c))
                {
                    if (gotColon)
                    {
                        hostName = default;
                        exception = Helper.CreateException(ExtractionErrorTag.UnexpectedChar, pos);
                        return 0;
                    }

                    canBeIPv6 = false;

                    canBeAscii = false;
                    currentAsciiLabelLength = 0;
                    nothingButPeriodsAndDigits = false;
                }
                else if (c.IsDecimalDigit())
                {
                    if (canBeAscii)
                    {
                        currentAsciiLabelLength++;
                    }
                }
                else
                {
                    // wrong char for a host name.
                    hostName = default;
                    exception = Helper.CreateException(ExtractionErrorTag.UnexpectedChar, pos);
                    return 0;
                }

                if (currentAsciiLabelLength > Helper.MaxAsciiLabelLength)
                {
                    hostName = default;
                    exception = Helper.CreateException(ExtractionErrorTag.DomainLabelTooLong, pos);
                    return 0;
                }

                prevChar = c;
                pos++;
            }

            if (gotColon)
            {
                if (!canBeIPv6)
                {
                    exception = Helper.CreateException(ExtractionErrorTag.InvalidIPv6Address, 0);
                    hostName = default;
                    return 0;
                }

                // IPv6
                var parsed = IPAddress.TryParse(input[..pos], out var ipAddress);

                if (parsed)
                {
                    hostName = new HostName(HostNameKind.IPv6, ipAddress.ToString());
                    exception = null;
                    return pos;
                }
                else
                {
                    hostName = default;
                    exception = Helper.CreateException(ExtractionErrorTag.InvalidIPv6Address, 0);
                    return 0;
                }
            }

            if (nothingButPeriodsAndDigits)
            {
                if (periodCount == 3)
                {
                    // might be IPv4
                    var parsed = IPAddress.TryParse(input[..pos], out var ipAddress);

                    if (parsed)
                    {
                        hostName = new HostName(HostNameKind.IPv4, ipAddress.ToString());
                        exception = null;
                        return pos;
                    }
                    else
                    {
                        exception = Helper.CreateException(ExtractionErrorTag.InvalidIPv4Address, 0);
                        hostName = default;
                        return 0;
                    }
                }
                else
                {
                    // only numeric segments, but their count not equal to 4
                    hostName = default;
                    exception = Helper.CreateException(ExtractionErrorTag.InvalidHostName, 0);
                    return 0;
                }
            }

            // ascii domain name
            if (canBeAscii)
            {
                hostName = new HostName(HostNameKind.Regular, input[..pos].ToString().ToLowerInvariant());
                exception = null;
                return pos;
            }

            // unicode domain name
            try
            {
                var ascii = Idn.GetAscii(input[..pos].ToString().ToLowerInvariant());
                hostName = new HostName(HostNameKind.Internationalized, ascii);
                exception = null;
                return pos;
            }
            catch
            {
                hostName = default;
                exception = Helper.CreateException(ExtractionErrorTag.InvalidHostName, 0);
                return 0;
            }
        }

        #endregion

        #region IEquatable<HostName> Members

        public bool Equals(HostName other)
        {
            return
                this.Kind == other.Kind &&
                this.Value == other.Value;
        }

        #endregion

        #region Overridden

        public override bool Equals(object obj)
        {
            return obj is HostName other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int)this.Kind, this.Value);
        }

        public override string ToString() => this.Value;

        #endregion
    }
}
