using System.Collections.Generic;
using System.Text;

namespace TauCode.Data.Text.Tests.TextDataExtractor.SqlIdentifier;

public class SqlIdentifierExtractorTestDto
{
    public int? Index { get; set; }

    public string TestInput { get; set; }
    public string TestTerminatingChars { get; set; }
    public int? TestMaxConsumption { get; set; }
    public IList<SqlIdentifierDelimiter> TestDelimiters { get; set; }
    public IList<string> TestReservedWords { get; set; }

    public SqlIdentifierDto ExpectedValue { get; set; }
    public TextDataExtractionResultDto ExpectedResult { get; set; }
    public string ExpectedErrorMessage { get; set; }
    public string ExpectedValueString { get; set; }

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
