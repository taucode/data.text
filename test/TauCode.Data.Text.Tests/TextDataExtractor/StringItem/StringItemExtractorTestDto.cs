using System.Collections.Generic;
using System.Text;

namespace TauCode.Data.Text.Tests.TextDataExtractor.StringItem;

public class StringItemExtractorTestDto
{
    public int? Index { get; set; }

    public string TestInput { get; set; }
    public List<string> TestItems { get; set; } = new();
    public bool TestIgnoreCase { get; set; }
    public string TestTerminatingChars { get; set; }

    public string ExpectedValue { get; set; }
    public TextDataExtractionResultDto ExpectedResult { get; set; }
    public string ExpectedErrorMessage { get; set; }

    public int ExpectedConsumed { get; set; }
    public string ExpectedStringItem { get; set; }
    public int? ExpectedErrorCode { get; set; }

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