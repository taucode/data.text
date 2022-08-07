using System.Text;
using TauCode.Data.Text.Exceptions;
using TauCode.Data.Text.TextDataExtractors;

namespace TauCode.Data.Text;

public class EmailAddress : IEquatable<EmailAddress>
{
    #region Static

    private static readonly EmailAddressExtractor Extractor = new EmailAddressExtractor((input, index) => false);

    #endregion

    #region Fields

    public readonly string LocalPart;
    public readonly HostName Domain;

    private string? _stringRepresentation;
    private bool _stringRepresentationBuilt;

    #endregion

    #region ctor

    internal EmailAddress(string localPart, HostName hostName)
    {
        this.LocalPart = localPart;
        this.Domain = hostName;
    }

    #endregion

    #region Private

    private string? BuildValue()
    {
        if (this.Domain.Value == null) // domain is default(HostName), which should not happen, actually
        {
            return null;
        }

        var sb = new StringBuilder();

        sb.Append(this.LocalPart);
        sb.Append("@");

        string format;

        switch (this.Domain.Kind)
        {
            case HostNameKind.Regular:
            case HostNameKind.Internationalized:
                format = "{0}";
                break;

            case HostNameKind.IPv4:
                format = "[{0}]";
                break;

            case HostNameKind.IPv6:
                format = "[IPv6:{0}]";
                break;

            default:
                throw new FormatException("Cannot build email value.");
        }

        sb.AppendFormat(format, this.Domain.Value);
        var result = sb.ToString();
        return result;
    }

    #endregion

    #region Internal

    internal static int CalculateLength(int localPartLength, HostName domain)
    {
        var result = 0;

        result += localPartLength;
        result += 1; // '@'
        switch (domain.Kind)
        {
            case HostNameKind.Regular:
            case HostNameKind.Internationalized:
                result += domain.Value.Length;
                break;

            case HostNameKind.IPv4:
                result += domain.Value.Length + 2; // []
                break;

            case HostNameKind.IPv6:
                result += domain.Value.Length + 7; // [IPv6:]
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        return result;
    }

    #endregion

    #region Parsing

    public static EmailAddress Parse(
        ReadOnlySpan<char> input)
    {
        var result = Extractor.TryExtract(input, out var value);
        if (result.ErrorCode.HasValue)
        {
            var message = Extractor.GetErrorMessage(result.ErrorCode.Value);
            throw new TextDataExtractionException(message, result.ErrorCode.Value, result.CharsConsumed);
        }

        return value!;
    }

    public static bool TryParse(
        ReadOnlySpan<char> input,
        out EmailAddress? emailAddress)
    {
        var result = Extractor.TryExtract(input, out emailAddress);
        return result.ErrorCode == null;
    }

    #endregion

    #region IEquatable<EmailAddress> Members

    public bool Equals(EmailAddress? other)
    {
        if (other == null)
        {
            return false;
        }

        return
            this.LocalPart == other.LocalPart &&
            this.Domain.Equals(other.Domain);
    }

    #endregion

    #region Overridden

    public override bool Equals(object? obj)
    {
        return obj is EmailAddress other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.LocalPart, this.Domain);
    }

    public override string? ToString()
    {
        if (!_stringRepresentationBuilt)
        {
            _stringRepresentation = this.BuildValue();
            _stringRepresentationBuilt = true;
        }

        return _stringRepresentation;
    }

    #endregion
}