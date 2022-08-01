namespace TauCode.Data.Text.TextDataExtractors
{
    public class TermExtractor : TextDataExtractorBase<string>
    {
        public TermExtractor(
            TerminatingDelegate? terminator = null)
            : base(
                Helper.Constants.Term.DefaultMaxConsumption,
                terminator)
        {
        }

        protected override TextDataExtractionResult TryExtractImpl(
            ReadOnlySpan<char> input,
            out string? value)
        {
            var pos = 0;
            char? prevChar = null;

            value = default;

            while (true)
            {
                if (pos == input.Length)
                {
                    if (prevChar == '-')
                    {
                        return new TextDataExtractionResult(
                            pos,
                            TextDataExtractionErrorCodes.UnexpectedEnd);
                    }

                    break;
                }

                var c = input[pos];

                if (c.IsDecimalDigit())
                {
                    if (pos == 0)
                    {
                        return new TextDataExtractionResult(0, TextDataExtractionErrorCodes.UnexpectedCharacter);
                    }

                    // ok
                }
                else if (c.IsLatinLetterInternal())
                {
                    // go on
                }
                else if (c == '-')
                {
                    if (pos == 0 || prevChar == '-')
                    {
                        return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.UnexpectedCharacter);
                    }

                    // ok.
                }
                else if (this.IsTermination(input, pos))
                {
                    break;
                }
                else
                {
                    return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.UnexpectedCharacter);
                }

                prevChar = c;
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

            value = input[..pos].ToString();
            return new TextDataExtractionResult(pos, null);
        }
    }
}

