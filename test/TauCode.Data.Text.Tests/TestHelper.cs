using System.Text;
using System.Text.RegularExpressions;
using TauCode.Data.Text.Exceptions;
using TauCode.Extensions;

namespace TauCode.Data.Text.Tests;

internal static class TestHelper
{
    #region Dto Extensions

    public static SemanticVersionDto ToDto(this Text.SemanticVersion semanticVersion)
    {
        return new SemanticVersionDto
        {
            Major = semanticVersion.Major,
            Minor = semanticVersion.Minor,
            Patch = semanticVersion.Patch,
            PreRelease = semanticVersion.PreRelease,
            BuildMetadata = semanticVersion.BuildMetadata,
        };
    }

    public static EmojiDto ToDto(this Text.Emoji emoji)
    {
        return new EmojiDto
        {
            Value = emoji.Value,
            Name = emoji.Name,
        };
    }

    public static HostNameDto ToDto(this Text.HostName hostName)
    {
        return new HostNameDto
        {
            Kind = hostName.Kind,
            Value = hostName.Value,
        };
    }

    public static EmailAddressDto ToDto(this Text.EmailAddress emailAddress)
    {
        return new EmailAddressDto
        {
            LocalPart = emailAddress.LocalPart,
            Domain = emailAddress.Domain.ToDto(),
        };
    }

    public static SqlIdentifierDto ToDto(this Text.SqlIdentifier sqlIdentifier)
    {
        return new SqlIdentifierDto
        {
            Value = sqlIdentifier.Value,
            Delimiter = sqlIdentifier.Delimiter,
        };
    }

    public static TextDataExtractionExceptionDto ToDto(this TextDataExtractionException ex)
    {
        return new TextDataExtractionExceptionDto
        {
            Message = ex.Message,
            CharsConsumed = ex.CharsConsumed,
            ErrorCode = ex.ErrorCode,
        };
    }

    public static TextDataExtractionResultDto ToDto(this TextDataExtractionResult extractionResult)
    {
        return new TextDataExtractionResultDto
        {
            CharsConsumed = extractionResult.CharsConsumed,
            ErrorCode = extractionResult.ErrorCode,
        };
    }

    #endregion

    internal static string TransformTestString(string? s)
    {
        if (s == null)
        {
            throw new ArgumentNullException(nameof(s));
        }

        const string pattern = @"\{@([^:]+)\:(\d+)@\}";
        var result = Regex.Replace(s, pattern, TestHelper.PatternEvaluator);

        return result;
    }

    private static string PatternEvaluator(Match match)
    {

        var txt = match.Groups[1].Value;
        var lenText = match.Groups[2].Value;

        var len = int.Parse(lenText);
        var replacement = RepeatString(txt, len);

        return replacement;
    }

    private static string RepeatString(string txt, int len)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < len; i++)
        {
            sb.Append(txt);
        }

        return sb.ToString();
    }

    public static string LoadResource(string resourceName)
    {
        var txt = typeof(TestHelper).Assembly.GetResourceText(resourceName, true);
        return txt;
    }

    public static bool IsWhiteSpace(ReadOnlySpan<char> input, int pos)
    {
        var c = input[pos];
        var result = c.IsIn(' ', '\t', '\r', '\n');
        return result;
    }
}
