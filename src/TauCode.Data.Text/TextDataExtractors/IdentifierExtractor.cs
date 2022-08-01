namespace TauCode.Data.Text.TextDataExtractors
{
    public class IdentifierExtractor : TextDataExtractorBase<string>
    {
        public IdentifierExtractor(
            Func<string, bool>? reservedWordPredicate,
            TerminatingDelegate? terminator = null)
            : base(
                Helper.Constants.Identifier.DefaultMaxConsumption,
                terminator)
        {
            this.ReservedWordPredicate = reservedWordPredicate;
        }

        public Func<string, bool>? ReservedWordPredicate { get; }

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
                else if (this.IsTermination(input, pos))
                {
                    break;
                }
                else
                {
                    return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.UnexpectedCharacter);
                }

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

            var isReservedWord = this.ReservedWordPredicate?.Invoke(value) ?? false;
            if (isReservedWord)
            {
                value = default;
                return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.ValueIsReservedWord);
            }

            return new TextDataExtractionResult(pos, null);
        }
    }
}
