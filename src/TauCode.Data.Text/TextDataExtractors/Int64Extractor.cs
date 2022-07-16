using System;

namespace TauCode.Data.Text.TextDataExtractors
{
    public class Int64Extractor : TextDataExtractorBase<long>
    {
        public Int64Extractor(TerminatingDelegate terminator = null)
            : base(
                Helper.Constants.Int64.MaxConsumption,
                terminator)
        {
        }

        protected override TextDataExtractionResult TryExtractImpl(
            ReadOnlySpan<char> input,
            out long value)
        {
            var pos = 0;

            value = default;

            while (true)
            {
                if (pos > Helper.Constants.Int64.MaxConsumption)
                {
                    return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.FailedToExtractInt64);
                }

                if (pos == input.Length)
                {
                    break;
                }

                var c = input[pos];
                if (c == '-' || c == '+')
                {
                    if (pos == 0)
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
                }
                else if (c.IsDecimalDigit())
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
            }

            if (pos == 0)
            {
                return new TextDataExtractionResult(0, TextDataExtractionErrorCodes.UnexpectedEnd);
            }

            input = input[..pos];
            var parsed = long.TryParse(input, out value);

            if (parsed)
            {
                return new TextDataExtractionResult(pos, null);
            }

            return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.FailedToExtractInt64);
        }
    }
}
