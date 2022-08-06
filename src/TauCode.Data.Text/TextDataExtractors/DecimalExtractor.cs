namespace TauCode.Data.Text.TextDataExtractors;

public class DecimalExtractor : TextDataExtractorBase<decimal>
{
    private static readonly HashSet<char> DecimalChars;

    static DecimalExtractor()
    {
        DecimalChars = new HashSet<char>("+-0123456789.");
    }

    public DecimalExtractor(TerminatingDelegate? terminator = null)
        : base(
            Helper.Constants.Decimal.DefaultMaxConsumption,
            terminator)
    {
    }

    protected override TextDataExtractionResult TryExtractImpl(
        ReadOnlySpan<char> input,
        out decimal value)
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
            if (DecimalChars.Contains(c))
            {
                // ok
            }
            else if (this.IsTermination(input, pos))
            {
                break;
            }
            else
            {
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
            return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.UnexpectedEnd);
        }

        input = input[..pos];
        var parsed = decimal.TryParse(input, out value);

        if (parsed)
        {
            return new TextDataExtractionResult(pos, null);
        }

        return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.FailedToExtractDecimal);
    }
}