using System;

namespace TauCode.Data.Text.TextDataExtractors
{
    public class KeyExtractor : TextDataExtractorBase<string>
    {
        public KeyExtractor(TerminatingDelegate terminator = null)
            : base(
                Helper.Constants.Key.DefaultMaxConsumption,
                terminator)
        {
        }

        protected override TextDataExtractionResult TryExtractImpl(
            ReadOnlySpan<char> input,
            out string value)
        {
            var pos = 0;

            value = default;

            char? prevChar = null;
            char? prevPrevChar = null;

            while (true)
            {
                if (pos == input.Length)
                {
                    if (prevChar == '-')
                    {
                        return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.FailedToExtractKey);
                    }

                    break;
                }

                var c = input[pos];

                if (c == '-')
                {
                    if (pos == 0)
                    {
                        // ok
                    }
                    else
                    {
                        if (prevChar == '-')
                        {
                            if (prevPrevChar == '-')
                            {
                                return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.UnexpectedCharacter);
                            }

                            if (pos == 1)
                            {
                                // ok
                            }
                            else
                            {
                                return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.UnexpectedCharacter);
                            }
                        }
                    }
                }
                else
                {
                    if (pos == 0)
                    {
                        return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.UnexpectedCharacter);
                    }

                    if (c.IsLatinLetterInternal() || c.IsDecimalDigit())
                    {
                        // ok
                    }
                    else if (this.Terminator(input, pos))
                    {
                        if (prevChar == '-')
                        {
                            return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.FailedToExtractKey);
                        }

                        break;
                    }
                    else
                    {
                        return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.UnexpectedCharacter);
                    }
                }

                prevPrevChar = prevChar;
                prevChar = c;

                pos++;

                this.CheckConsumption(pos); // todo_deferred ut
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
