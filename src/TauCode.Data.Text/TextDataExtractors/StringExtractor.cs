using System;

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

        protected override TextDataExtractionResult TryExtractImpl(ReadOnlySpan<char> input, out string value)
        {
            var pos = 0;

            while (true)
            {
                if (pos == input.Length)
                {
                    break;
                }

                if (this.Terminator(input, pos))
                {
                    break;
                }

                pos++;
            }

            var str = input[..pos].ToString();
            value = str;

            return new TextDataExtractionResult(pos, null);
        }
    }
}
