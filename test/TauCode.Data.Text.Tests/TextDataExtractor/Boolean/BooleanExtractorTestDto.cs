using System.Text;

namespace TauCode.Data.Text.Tests.TextDataExtractor.Boolean;

public class BooleanExtractorTestDto
{
    public int? Index { get; set; }

    public string? TestInput { get; set; }
    public string? TestTerminatingChars { get; set; }
    public int? TestMaxConsumption { get; set; }

    public bool ExpectedValue { get; set; }
    public TextDataExtractionResultDto ExpectedResult { get; set; } = default!; // deserialized from JSON = default!; // deserialized from json
    public string? ExpectedErrorMessage { get; set; }

    public string Comment { get; set; } = default!; // deserialized from json

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
