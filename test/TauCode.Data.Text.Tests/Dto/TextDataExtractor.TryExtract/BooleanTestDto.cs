using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TauCode.Data.Text.Tests.Dto.TextDataExtractor.TryExtract;

public class BooleanTestDto
{
    public int? Index { get; set; }
    public string TestInput { get; set; }
    public string TestTerminatingChars { get; set; }
    public int ExpectedResult { get; set; }
    public bool? ExpectedBoolean { get; set; }
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
