namespace TauCode.Data.Text.TextDataExtractors
{
    public class StringExtractor : TextDataExtractorBase<string>
    {
        public StringExtractor(TerminatingDelegate terminator)
            : base(
                null,
                terminator)
        {
        }

        protected override TextDataExtractionResult TryExtractImpl(ReadOnlySpan<char> input, out string? value)
        {
            var pos = 0;
            value = default;

            while (true)
            {
                if (pos == input.Length)
                {
                    break;
                }

                if (this.IsTermination(input, pos))
                {
                    break;
                }

                pos++;

                if (this.IsOutOfCapacity(pos))
                {
                    return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.InputIsTooLong);
                }
            }

            var str = input[..pos].ToString();
            value = str;

            return new TextDataExtractionResult(pos, null);
        }
    }
}
