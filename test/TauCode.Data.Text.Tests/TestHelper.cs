using System.Text;
using System.Text.RegularExpressions;
using TauCode.Extensions;

namespace TauCode.Data.Text.Tests;

internal static class TestHelper
{
    internal static string TransformTestString(string s)
    {
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
}
