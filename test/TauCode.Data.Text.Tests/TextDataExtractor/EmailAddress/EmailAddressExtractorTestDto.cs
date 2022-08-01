using System.Text;

namespace TauCode.Data.Text.Tests.TextDataExtractor.EmailAddress;

public class EmailAddressExtractorTestDto
{
    public int? Index { get; set; }

    public string TestInput { get; set; } = default!; // deserialized from JSON
    public string? TestTerminatingChars { get; set; }
    public int? TestMaxConsumption { get; set; }

    public EmailAddressDto? ExpectedValue { get; set; }
    public TextDataExtractionResultDto ExpectedResult { get; set; } = default!; // deserialized from JSON = default!; // deserialized from JSON
    public string? ExpectedErrorMessage { get; set; }
    public string? ExpectedValueString { get; set; }

    public string Comment { get; set; } = default!; // deserialized from JSON

    public override string ToString()
    {
        var sb = new StringBuilder();

        if (this.Index.HasValue)
        {
            sb.Append($"{this.Index:0000} ");
        }

        sb.Append($"'{this.TestInput}'");
        return sb.ToString();
    }
}
