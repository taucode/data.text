using System;
using System.Collections.Generic;

namespace TauCode.Data.Text.TextDataExtractors
{
    public class DateTimeOffsetExtractor : TextDataExtractorBase<DateTimeOffset>
    {

        private static readonly HashSet<char> DateTimeOffsetChars;

        static DateTimeOffsetExtractor()
        {
            DateTimeOffsetChars = new HashSet<char>("+-0123456789.:TZ");
        }

        public DateTimeOffsetExtractor(TerminatingDelegate terminator = null)
            : base(
                Helper.Constants.DateTimeOffset.MaxConsumption,
                terminator)
        {
        }

        protected override TextDataExtractionResult TryExtractImpl(
            ReadOnlySpan<char> input,
            out DateTimeOffset value)
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
                if (DateTimeOffsetChars.Contains(c))
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

                if (pos == Helper.Constants.DateTimeOffset.MaxConsumption)
                {
                    return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.InputTooLong);
                }

                pos++;

                this.CheckConsumption(pos); // todo_deferred ut
            }

            if (pos == 0)
            {
                return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.UnexpectedEnd);
            }

            var parseInput = input[..pos];
            var parsed = DateTimeOffset.TryParse(parseInput, out value);

            if (parsed)
            {
                return new TextDataExtractionResult(pos, null);
            }
            else
            {
                return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.FailedToExtractDateTimeOffset);
            }
        }
    }
}
