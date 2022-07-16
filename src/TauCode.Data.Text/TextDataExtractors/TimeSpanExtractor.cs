using System;
using System.Collections.Generic;
using System.Globalization;

namespace TauCode.Data.Text.TextDataExtractors
{
    public class TimeSpanExtractor : TextDataExtractorBase<TimeSpan>
    {
        private static readonly HashSet<char> TimeSpanChars;

        static TimeSpanExtractor()
        {
            TimeSpanChars = new HashSet<char>("+-0123456789.:");
        }

        public TimeSpanExtractor(
            TerminatingDelegate terminator = null)
            : base(
                Helper.Constants.TimeSpan.TimeSpanMaxConsumption,
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
                else if (this.Terminator(input, pos))
                {
                    break;
                }
                else
                {
                    value = default;
                    return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.UnexpectedCharacter);
                }

                if (pos == this.MaxConsumption) // todo_deferred ut
                {
                    value = default;
                    return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.InputTooLong);
                }

                pos++;
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
}
