using System;

namespace TauCode.Data.Text.TextDataExtractors
{
    public class Int32Extractor : TextDataExtractorBase<int>
    {
        public Int32Extractor(TerminatingDelegate terminator = null)
            : base(
                Helper.Constants.Int32.MaxConsumption,
                terminator)
        {
        }

        protected override TextDataExtractionResult TryExtractImpl(
            ReadOnlySpan<char> input,
            out int value)
        {
            var pos = 0;

            while (true)
            {
                if (pos > Helper.Constants.Int32.MaxConsumption)
                {
                    value = default;
                    return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.InputTooLong); // todo_deferred ut
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
                        value = default;
                        return new TextDataExtractionResult(
                            pos,
                            TextDataExtractionErrorCodes.UnexpectedCharacter); // todo_deferred ut
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
                    value = default;
                    return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.UnexpectedCharacter);
                }

                pos++;
            }

            if (pos == 0)
            {
                value = default;
                return new TextDataExtractionResult(0, TextDataExtractionErrorCodes.UnexpectedEnd);
            }

            input = input[..pos];
            var parsed = int.TryParse(input, out value);

            if (parsed)
            {
                return new TextDataExtractionResult(pos, null);
            }

            return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.FailedToExtractInt32);
        }
    }
}
