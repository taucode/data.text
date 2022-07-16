using System;

namespace TauCode.Data.Text.TextDataExtractors
{
    public class IdentifierExtractor : TextDataExtractorBase<string>
    {
        public IdentifierExtractor(
            int maxConsumption = Helper.Constants.Identifier.DefaultMaxConsumption,
            TerminatingDelegate terminator = null)
            : base(maxConsumption, terminator)
        {
        }

        protected override TextDataExtractionResult TryExtractImpl(ReadOnlySpan<char> input, out string value)
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

                if (c.IsDecimalDigit())
                {
                    if (pos == 0)
                    {
                        return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.UnexpectedCharacter);
                    }
                }
                else if (
                    c.IsLatinLetterInternal() ||
                    c == '_' ||
                    false)
                {
                    // ok
                }
                else if (this.Terminator(input, pos))
                {
                    break;
                }
                else
                {
                    return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.UnexpectedCharacter);
                }

                pos++;

                if (pos > this.MaxConsumption)
                {
                    return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.InputTooLong);
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
