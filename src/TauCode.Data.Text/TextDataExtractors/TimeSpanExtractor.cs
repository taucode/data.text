using System.Globalization;

namespace TauCode.Data.Text.TextDataExtractors;

public class TimeSpanExtractor : TextDataExtractorBase<TimeSpan>
{
    private static readonly HashSet<char> TimeSpanChars;

    static TimeSpanExtractor()
    {
        TimeSpanChars = new HashSet<char>("+-0123456789.:");
    }

    public TimeSpanExtractor(
        TerminatingDelegate? terminator = null)
        : base(
            Helper.Constants.TimeSpan.DefaultMaxConsumption,
            terminator)
    {
    }

    protected override TextDataExtractionResult TryExtractImpl(
        ReadOnlySpan<char> input,
        out TimeSpan value)
    {
        var pos = 0;

        value = default;
        while (true)
        {
            if (pos == input.Length)
            {
                break;
            }

            var c = input[pos];
            if (TimeSpanChars.Contains(c))
            {
                // ok
            }
            else if (this.IsTermination(input, pos))
            {
                break;
            }
            else
            {
                value = default;
                return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.UnexpectedCharacter);
            }

            pos++;

            if (this.IsOutOfCapacity(pos))
            {
                return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.InputIsTooLong);
            }
        }

        if (pos == 0)
        {
            value = default;
            return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.UnexpectedEnd);
        }

        var parseInput = input[..pos];
        var parsed = TimeSpan.TryParse(parseInput, CultureInfo.InvariantCulture, out value);

        if (parsed)
        {
            return new TextDataExtractionResult(pos, null);
        }
        else
        {
            return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.FailedToExtractTimeSpan);
        }
    }
}