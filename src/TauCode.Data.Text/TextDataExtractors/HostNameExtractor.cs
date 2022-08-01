using System.Globalization;
using System.Net;

namespace TauCode.Data.Text.TextDataExtractors
{
    public class HostNameExtractor : TextDataExtractorBase<HostName>
    {
        #region Constants & Static


        private static readonly IdnMapping Idn; // IdnMapping type is thread-safe

        static HostNameExtractor()
        {
            Idn = new IdnMapping
            {
                UseStd3AsciiRules = true,
            };
        }

        #endregion

        public HostNameExtractor(TerminatingDelegate? terminator = null)
            : base(
                Helper.Constants.HostName.DefaultMaxConsumption,
                terminator)
        {
        }

        protected override TextDataExtractionResult TryExtractImpl(ReadOnlySpan<char> input, out HostName value)
        {
            var canBeIPv6 = true;
            var periodCount = 0; // period is '.'
            var gotColon = false;
            var nothingButPeriodsAndDigits = true;
            var canBeAscii = true;
            var currentAsciiLabelLength = 0; // see Helper.MaxAsciiLabelLength to find out what label is.

            char? prevChar = null;
            var pos = 0;
            value = default;

            while (true)
            {
                if (pos == input.Length)
                {
                    if (
                        prevChar == '.' ||
                        prevChar == '-' ||
                        false
                    )
                    {
                        // these chars cannot be last ones
                        return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.UnexpectedEnd);
                    }

                    break;
                }

                if (canBeIPv6 && gotColon && pos == Helper.Constants.HostName.MaxIPv6Length)
                {
                    // got IPv6 here, cannot consume more chars
                    if (this.IsTermination(input, pos))
                    {
                        // got terminating char, let's try parse IPv6 below
                        break;
                    }
                    else
                    {
                        // looks like bad ipv6 to me
                        return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.InvalidIPv6Address);
                    }
                }

                var c = input[pos];

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
                        return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.UnexpectedCharacter);
                    }

                    periodCount++;

                    if (canBeAscii)
                    {
                        // '.' cuts off previous label, if there was any. See Helper.MaxAsciiLabelLength to find out what label is.
                        currentAsciiLabelLength = 0;
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
                            return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.UnexpectedCharacter);
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
                        return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.UnexpectedCharacter);
                    }

                    if (periodCount > 0)
                    {
                        // ':' cannot follow a '.'
                        return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.UnexpectedCharacter);
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
                        return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.UnexpectedCharacter);
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
                        return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.UnexpectedCharacter);
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
                else if (this.IsTermination(input, pos))
                {
                    if (pos == 0)
                    {
                        return new TextDataExtractionResult(0, TextDataExtractionErrorCodes.UnexpectedEnd);
                    }

                    // got terminating char
                    if (
                        prevChar == '.' ||
                        prevChar == '-' ||
                        false
                    )
                    {
                        // these chars cannot be last ones
                        return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.UnexpectedEnd);
                    }

                    break;
                }
                else
                {
                    // wrong char for a host name.
                    return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.UnexpectedCharacter);
                }

                prevChar = c;
                pos++;

                if (currentAsciiLabelLength > Helper.Constants.HostName.MaxAsciiLabelLength)
                {
                    return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.DomainLabelIsTooLong);
                }

                if (this.IsOutOfCapacity(pos))
                {
                    return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.InputIsTooLong);
                }
            }

            if (gotColon)
            {
                // IPv6
                var parsed = IPAddress.TryParse(input[..pos], out var ipAddress);

                if (parsed)
                {
                    value = new HostName(HostNameKind.IPv6, ipAddress!.ToString());
                    return new TextDataExtractionResult(pos, null);
                }
                else
                {
                    return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.InvalidIPv6Address);
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
                        value = new HostName(HostNameKind.IPv4, ipAddress!.ToString());
                        return new TextDataExtractionResult(pos, null);
                    }
                    else
                    {
                        return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.InvalidIPv4Address);
                    }
                }
                else
                {
                    return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.InvalidHostName);
                }
            }

            // ascii domain name
            if (canBeAscii)
            {
                value = new HostName(HostNameKind.Regular, input[..pos].ToString().ToLowerInvariant());
                return new TextDataExtractionResult(pos, null);
            }

            // unicode domain name
            try
            {
                var ascii = Idn.GetAscii(input[..pos].ToString().ToLowerInvariant());
                value = new HostName(HostNameKind.Internationalized, ascii);
                return new TextDataExtractionResult(pos, null);
            }
            catch
            {
                return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.InvalidHostName);
            }
        }
    }
}
