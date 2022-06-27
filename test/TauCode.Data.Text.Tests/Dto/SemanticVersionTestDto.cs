using System.Text;

namespace TauCode.Data.Text.Tests.Dto;

public class SemanticVersionTestDto
{
    public int? Index { get; set; }
    public string TestSemanticVersion { get; set; }
    public int ExpectedResult { get; set; }
    public string ExpectedSemanticVersion { get; set; }
    public int? ExpectedMajor { get; set; }
    public int? ExpectedMinor { get; set; }
    public int? ExpectedPatch { get; set; }
    public string ExpectedPreRelease { get; set; }
    public string ExpectedBuildMetadata { get; set; }
    public string Comment { get; set; }
    public ExceptionDto ExpectedException { get; set; }


    public override string ToString()
    {
        var sb = new StringBuilder();

        if (this.Index.HasValue)
        {
            sb.Append($"{this.Index:0000} ");
        }

        sb.Append($"'{this.TestSemanticVersion}'");
        return sb.ToString();
    }
}
