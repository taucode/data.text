using System.Text;

namespace TauCode.Data.Text.Tests.Dto;

public class EmailAddressTestDto
{
    public int? Index { get; set; }
    public string TestEmailAddress { get; set; }
    public int? ExpectedResult { get; set; }
    public string ExpectedEmailAddress { get; set; }
    public string Comment { get; set; }
    public ExceptionDto ExpectedException { get; set; }

    public override string ToString()
    {
        var sb = new StringBuilder();

        if (this.Index.HasValue)
        {
            sb.Append($"{this.Index:0000} ");
        }

        sb.Append($"'{this.TestEmailAddress}'");
        return sb.ToString();
    }
}
