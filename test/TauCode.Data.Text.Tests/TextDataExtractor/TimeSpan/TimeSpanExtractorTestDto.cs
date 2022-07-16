using System.Text;

namespace TauCode.Data.Text.Tests.TextDataExtractor.TimeSpan;

public class TimeSpanExtractorTestDto
{
    public int? Index { get; set; }

    public string TestInput { get; set; }
    public string TestTerminatingChars { get; set; }

    public System.TimeSpan ExpectedValue { get; set; }
    public TextDataExtractionResultDto ExpectedResult { get; set; }
    public string ExpectedErrorMessage { get; set; }

    public string Comment { get; set; }

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
