using TauCode.Extensions;

namespace TauCode.Data.Text;

internal static class Helper
{
    #region Constants

    internal static class Constants
    {
        internal static class HostName
        {
            /// <summary>
            /// Label is a part of dot-separated ascii domain name, e.g.
            /// in case of domain name 'cor1.2.some-site.com',
            /// labels are: 'cor1', '2', 'some-site', 'com'.
            /// </summary>
            internal const int MaxAsciiLabelLength = 63;

            internal const int MaxLength = 253;
            internal const int DefaultMaxConsumption = 253;

            // "1111:2222:3333:4444:5555:6666:123.123.123.123".Length
            internal const int MaxIPv6Length = 45;
        }

        internal static class TimeSpan
        {
            internal const int DefaultMaxConsumption = 26; // "-10675199.02:48:05.4775808".Length;
        }

        internal static class EmailAddress
        {
            internal const int DefaultMaxConsumption = 1000; // with all comments and folding whitespaces
            internal const int MaxLocalPartLength = 64;
            internal const int MaxCleanEmailAddressLength = 254;
            internal const int FoldingWhiteSpaceLength = 3;
        }

        internal static class FilePath
        {
            internal const int MaxLength = 260;
            internal const int DefaultMaxConsumption = 260;
        }

        internal static class DateTimeOffset
        {
            // "9999-12-31T23:59:59.9999999+00:00".Length
            internal const int DefaultMaxConsumption = 33;
        }

        internal static class Key
        {
            internal const int DefaultMaxConsumption = 200;
        }

        internal static class Decimal
        {
            // "79228162514264337593543950335".Length
            internal const int DefaultMaxConsumption = 29;
        }

        internal static class Double
        {
            // "-1.7976931348623157E+308".Length
            internal const int DefaultMaxConsumption = 24;
        }

        internal static class Int32
        {
            // int.MinValue.ToString(CultureInfo.InvariantCulture).Length
            internal const int DefaultMaxConsumption = 11;
        }

        internal static class Int64
        {
            // long.MinValue.ToString(CultureInfo.InvariantCulture).Length
            internal const int DefaultMaxConsumption = 20;
        }

        internal static class SemanticVersion
        {
            internal const int DefaultMaxConsumption = 500;
        }

        internal static class Term
        {
            internal const int DefaultMaxConsumption = 200;
        }

        internal static class Word
        {
            internal const int DefaultMaxConsumption = 200;
        }

        internal static class Identifier
        {
            internal const int DefaultMaxConsumption = 200;
        }

        internal static class SqlIdentifier
        {
            internal const int DefaultMaxConsumption = 200;
        }

        internal static class Uri
        {
            internal const int MaxLength = 2000;
            internal const int DefaultMaxConsumption = 2000;
        }
    }

    #endregion

    internal static bool IsLatinLetterInternal(this char c)
    {
        if (c >= 'a' && c <= 'z')
        {
            return true;
        }

        if (c >= 'A' && c <= 'Z')
        {
            return true;
        }

        return false;
    }

    internal static bool IsHexDigit(this char c)
    {
        if (c >= 'a' && c <= 'f')
        {
            return true;
        }

        if (c >= 'A' && c <= 'F')
        {
            return true;
        }

        if (c >= '0' && c <= '9')
        {
            return true;
        }

        return false;
    }

    internal static bool IsDecimalDigit(this char c)
    {
        if (c >= '0' && c <= '9')
        {
            return true;
        }

        return false;
    }

    internal static bool IsUnicodeInternal(this char c) => c >= 256;

    internal static string GetErrorMessage(int errorCode)
    {
        return errorCode switch
        {
            TextDataExtractionErrorCodes.InputIsTooLong => // 2
                "Input is too long.",

            TextDataExtractionErrorCodes.UnclosedString => // 3
                "Unclosed string.",

            TextDataExtractionErrorCodes.NewLineInString => // 4
                "New line in string.",

            TextDataExtractionErrorCodes.UnexpectedEnd => // 5
                "Unexpected end.",

            TextDataExtractionErrorCodes.BadEscape => // 6
                "Bad escape.",

            TextDataExtractionErrorCodes.UnexpectedCharacter =>  // 7
                "Unexpected character.",

            // Emoji: 100
            TextDataExtractionErrorCodes.NonEmojiCharacter => // 102
                "Non-emoji character.",

            // EmailAddress: 200
            TextDataExtractionErrorCodes.LocalPartIsTooLong => // 201
                "Local part is too long.",

            TextDataExtractionErrorCodes.EmailAddressIsTooLong => // 202
                "Email address is too long.",

            TextDataExtractionErrorCodes.IPv4MustBeEnclosedInBrackets => // 203
                "IPv4 address must be enclosed in '[' and ']'.",

            TextDataExtractionErrorCodes.EmptyLocalPart => // 204
                "Empty local part.",

            TextDataExtractionErrorCodes.EmptyString => // 205
                "Empty quoted string.",

            TextDataExtractionErrorCodes.InvalidIPv6Prefix => // 207
                "Invalid IPv6 prefix.",

            // HostName: 300
            TextDataExtractionErrorCodes.InvalidDomain =>  // 301
                "Invalid domain.",

            TextDataExtractionErrorCodes.HostNameIsTooLong => // 302
                "Host name is too long.",

            TextDataExtractionErrorCodes.InvalidHostName => // 303
                "Invalid host name.",

            TextDataExtractionErrorCodes.DomainLabelIsTooLong => // 304
                "Domain label is too long.",

            TextDataExtractionErrorCodes.InvalidIPv4Address => // 305
                "Invalid IPv4 address.",

            TextDataExtractionErrorCodes.InvalidIPv6Address => // 306
                "Invalid IPv6 address.",

            // int: 400
            TextDataExtractionErrorCodes.FailedToExtractInt32 =>  // 401
                $"Failed to extract {typeof(int).FullName}.",

            // long: 500
            TextDataExtractionErrorCodes.FailedToExtractInt64 => // 501
                $"Failed to extract {typeof(long).FullName}.",

            // decimal: 600
            TextDataExtractionErrorCodes.FailedToExtractDecimal => // 601
                $"Failed to extract {typeof(decimal).FullName}.",

            // double: 700
            TextDataExtractionErrorCodes.FailedToExtractDouble => // 701
                $"Failed to extract {typeof(double).FullName}.",

            // FilePath: 800
            TextDataExtractionErrorCodes.FailedToExtractFilePath => // 801
                $"Failed to extract file path.",

            TextDataExtractionErrorCodes.FilePathIsTooLong => // 802
                $"File path is too long.",

            // Key: 900
            TextDataExtractionErrorCodes.FailedToExtractKey => // 901
                $"Failed to extract key.",

            // DateTimeOffset: 1200
            TextDataExtractionErrorCodes.FailedToExtractDateTimeOffset => // 1201
                $"Failed to extract {typeof(DateTimeOffset).FullName}.",

            // Uri: 1300
            TextDataExtractionErrorCodes.FailedToExtractUri => // 1301
                $"Failed to extract {typeof(Uri).FullName}.",

            TextDataExtractionErrorCodes.UriIsTooLong => // 1302
                $"Uri is too long.",

            // TimeSpan: 1500
            TextDataExtractionErrorCodes.FailedToExtractTimeSpan => // 1501
                $"Failed to extract {typeof(TimeSpan).FullName}.",

            // SemanticVersion: 1600
            TextDataExtractionErrorCodes.FailedToExtractSemanticVersion => // 1601
                $"Failed to extract {typeof(SemanticVersion).FullName}.",

            // StringItem: 1700
            TextDataExtractionErrorCodes.ItemNotFound => // 1701
                "Item not found.",

            // Identifier: 1800
            TextDataExtractionErrorCodes.ValueIsReservedWord => // 1801
                "Value is a reserved word.",

            // unknown
            _ => $"Unknown error. Error code: {errorCode}."
        };
    }

    internal static bool IsInlineWhiteSpaceOrCaretControl(this char c) => IsInlineWhiteSpace(c) || IsCaretControl(c);

    internal static bool IsCaretControl(this char c) => c.IsIn('\r', '\n');

    internal static bool IsInlineWhiteSpace(this char c) => c.IsIn(' ', '\t');

    internal static char? ToOpeningDelimiter(this SqlIdentifierDelimiter delimiter)
    {
        switch (delimiter)
        {
            case SqlIdentifierDelimiter.None:
                return null;

            case SqlIdentifierDelimiter.Brackets:
                return '[';

            case SqlIdentifierDelimiter.DoubleQuotes:
                return '"';

            case SqlIdentifierDelimiter.BackQuotes:
                return '`';

            default:
                throw new ArgumentException($"Cannot resolve opening delimiter for '{delimiter}'.", nameof(delimiter));
        }
    }

    internal static char? ToClosingDelimiter(this SqlIdentifierDelimiter delimiter)
    {
        switch (delimiter)
        {
            case SqlIdentifierDelimiter.None:
                return null;

            case SqlIdentifierDelimiter.Brackets:
                return ']';

            case SqlIdentifierDelimiter.DoubleQuotes:
                return '"';

            case SqlIdentifierDelimiter.BackQuotes:
                return '`';

            default:
                throw new ArgumentException($"Cannot resolve opening delimiter for '{delimiter}'.", nameof(delimiter));
        }
    }

    internal static bool IsTermination<T>(this ITextDataExtractor<T> extractor, ReadOnlySpan<char> input, int pos)
    {
        return extractor.Terminator != null && extractor.Terminator(input, pos);
    }
}