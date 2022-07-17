using System;
using System.Text;

namespace TauCode.Data.Text.Tests;

public class TextDataExtractionResultDto : IEquatable<TextDataExtractionResultDto>
{
    public int CharsConsumed { get; set; }
    public int? ErrorCode { get; set; }

    public bool Equals(TextDataExtractionResultDto other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return CharsConsumed == other.CharsConsumed && ErrorCode == other.ErrorCode;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((TextDataExtractionResultDto)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(CharsConsumed, ErrorCode);
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append($"Consumed: {this.CharsConsumed}; ");
        sb.Append($"Error code: {this.ErrorCode} ({GetErrorCodeName(this.ErrorCode)}); ");

        var result = sb.ToString();
        return result;
    }

    private static string GetErrorCodeName(int? errorCode)
    {
        if (errorCode.HasValue)
        {
            switch (errorCode.Value)
            {
                // Common: 0
                case TextDataExtractionErrorCodes.InputIsTooLong:
                    return nameof(TextDataExtractionErrorCodes.InputIsTooLong);

                case TextDataExtractionErrorCodes.UnclosedString:
                    return nameof(TextDataExtractionErrorCodes.UnclosedString);

                case TextDataExtractionErrorCodes.NewLineInString:
                    return nameof(TextDataExtractionErrorCodes.NewLineInString);

                case TextDataExtractionErrorCodes.UnexpectedEnd:
                    return nameof(TextDataExtractionErrorCodes.UnexpectedEnd);

                case TextDataExtractionErrorCodes.BadEscape:
                    return nameof(TextDataExtractionErrorCodes.BadEscape);

                case TextDataExtractionErrorCodes.UnexpectedCharacter:
                    return nameof(TextDataExtractionErrorCodes.UnexpectedCharacter);

                // EmailAddress: 200
                case TextDataExtractionErrorCodes.LocalPartIsTooLong:
                    return nameof(TextDataExtractionErrorCodes.LocalPartIsTooLong);

                case TextDataExtractionErrorCodes.EmailAddressIsTooLong:
                    return nameof(TextDataExtractionErrorCodes.EmailAddressIsTooLong);

                case TextDataExtractionErrorCodes.IPv4MustBeEnclosedInBrackets:
                    return nameof(TextDataExtractionErrorCodes.IPv4MustBeEnclosedInBrackets);

                case TextDataExtractionErrorCodes.EmptyString:
                    return nameof(TextDataExtractionErrorCodes.EmptyString);

                case TextDataExtractionErrorCodes.InvalidIPv6Prefix:
                    return nameof(TextDataExtractionErrorCodes.InvalidIPv6Prefix);

                // HostName: 300
                case TextDataExtractionErrorCodes.HostNameIsTooLong:
                    return nameof(TextDataExtractionErrorCodes.HostNameIsTooLong);

                case TextDataExtractionErrorCodes.InvalidHostName:
                    return nameof(TextDataExtractionErrorCodes.InvalidHostName);

                case TextDataExtractionErrorCodes.DomainLabelIsTooLong:
                    return nameof(TextDataExtractionErrorCodes.DomainLabelIsTooLong);

                case TextDataExtractionErrorCodes.InvalidIPv4Address:
                    return nameof(TextDataExtractionErrorCodes.InvalidIPv4Address);

                case TextDataExtractionErrorCodes.InvalidIPv6Address:
                    return nameof(TextDataExtractionErrorCodes.InvalidIPv6Address);

                // int: 400
                case TextDataExtractionErrorCodes.FailedToExtractInt32:
                    return nameof(TextDataExtractionErrorCodes.FailedToExtractInt32);

                // long: 500
                case TextDataExtractionErrorCodes.FailedToExtractInt64:
                    return nameof(TextDataExtractionErrorCodes.FailedToExtractInt64);

                // decimal: 600
                case TextDataExtractionErrorCodes.FailedToExtractDecimal:
                    return nameof(TextDataExtractionErrorCodes.FailedToExtractDecimal);

                // double: 700
                case TextDataExtractionErrorCodes.FailedToExtractDouble:
                    return nameof(TextDataExtractionErrorCodes.FailedToExtractDouble);

                // FilePath: 800
                case TextDataExtractionErrorCodes.FailedToExtractFilePath:
                    return nameof(TextDataExtractionErrorCodes.FailedToExtractFilePath);

                case TextDataExtractionErrorCodes.FilePathIsTooLong:
                    return nameof(TextDataExtractionErrorCodes.FilePathIsTooLong);

                // Key: 900
                case TextDataExtractionErrorCodes.FailedToExtractKey:
                    return nameof(TextDataExtractionErrorCodes.FailedToExtractKey);

                // DateTimeOffset: 1200
                case TextDataExtractionErrorCodes.FailedToExtractDateTimeOffset:
                    return nameof(TextDataExtractionErrorCodes.FailedToExtractDateTimeOffset);

                // Uri: 1300
                case TextDataExtractionErrorCodes.FailedToExtractUri:
                    return nameof(TextDataExtractionErrorCodes.FailedToExtractUri);

                case TextDataExtractionErrorCodes.UriIsTooLong:
                    return nameof(TextDataExtractionErrorCodes.UriIsTooLong);

                // TimeSpan: 1500
                case TextDataExtractionErrorCodes.FailedToExtractTimeSpan:
                    return nameof(TextDataExtractionErrorCodes.FailedToExtractTimeSpan);

                // SemanticVersion: 1600
                case TextDataExtractionErrorCodes.FailedToExtractSemanticVersion:
                    return nameof(TextDataExtractionErrorCodes.FailedToExtractSemanticVersion);

                // StringItem: 1700
                case TextDataExtractionErrorCodes.ItemNotFound:
                    return nameof(TextDataExtractionErrorCodes.ItemNotFound);

                // Identifier: 1800
                case TextDataExtractionErrorCodes.ValueIsReservedWord:
                    return nameof(TextDataExtractionErrorCodes.ValueIsReservedWord);

                default:
                    throw new ArgumentOutOfRangeException(nameof(errorCode), $"Unresolved code: {errorCode}");
            }
        }

        return "<null>";
    }
}
