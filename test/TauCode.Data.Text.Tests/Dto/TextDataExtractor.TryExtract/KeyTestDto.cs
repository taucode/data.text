using System.Text;

namespace TauCode.Data.Text.Tests.Dto.TextDataExtractor.TryExtract;

public class KeyTestDto
{
    public int? Index { get; set; }
    public string TestInput { get; set; }
    public string TestTerminatingChars { get; set; }
    public int ExpectedResult { get; set; }
    public string ExpectedKey { get; set; }
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
