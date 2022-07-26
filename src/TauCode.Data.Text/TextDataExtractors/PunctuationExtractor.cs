using System;
using System.Collections.Generic;

// todo ut this
namespace TauCode.Data.Text.TextDataExtractors
{
    public sealed class PunctuationExtractor : TextDataExtractorBase<char>
    {
        public PunctuationExtractor(
            IEnumerable<char> punctuationChars,
            TerminatingDelegate terminator)
            : base(
                1,
                terminator)
        {
            if (punctuationChars == null)
            {
                throw new ArgumentNullException(nameof(punctuationChars));
            }

            this.PunctuationChars = new HashSet<char>(punctuationChars);
        }

        public HashSet<char> PunctuationChars { get; }

        public override int? MaxConsumption
        {
            get => 1;
            set => throw new InvalidOperationException();
        }

        protected override TextDataExtractionResult TryExtractImpl(ReadOnlySpan<char> input, out char value)
        {
            if (input.Length == 0)
            {
                value = default;
                return new TextDataExtractionResult(0, TextDataExtractionErrorCodes.UnexpectedEnd);
            }


            var c = input[0];
            if (this.PunctuationChars.Contains(c))
            {
                if (input.Length > 1)
                {
                    var isTermination = this.IsTermination(input, 1);
                    if (!isTermination)
                    {
                        value = default;
                        return new TextDataExtractionResult(1, TextDataExtractionErrorCodes.UnexpectedCharacter);
                    }
                }

                value = c;
                return new TextDataExtractionResult(1, null);
            }


            value = default;
            return new TextDataExtractionResult(0, TextDataExtractionErrorCodes.UnexpectedCharacter);
        }
    }
}
