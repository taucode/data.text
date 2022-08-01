namespace TauCode.Data.Text.EmailAddressSupport.SegmentExtractors
{
    internal class LocalPartWhiteSpaceExtractor : SegmentExtractor
    {
        internal override bool Accepts(ReadOnlySpan<char> input, EmailAddressExtractionContext context)
        {
            var c = input[context.Position];
            return c == ' ';
        }

        protected override TextDataExtractionResult ExtractImpl(
            ReadOnlySpan<char> input,
            EmailAddressExtractionContext context,
            out Segment? segment)
        {
            var length = input.Length;
            var start = context.Position;
            var pos = start + 1; // skip succeeded ' '

            while (true)
            {
                if (context.EmailAddressExtractor.IsOutOfCapacity(pos))
                {
                    segment = default;
                    return new TextDataExtractionResult(
                        context.EmailAddressExtractor.MaxConsumption!.Value + 1 - start,
                        TextDataExtractionErrorCodes.InputIsTooLong);
                }

                if (pos == length)
                {
                    // email address cannot end with white space
                    segment = default;
                    return new TextDataExtractionResult(pos - start, TextDataExtractionErrorCodes.UnexpectedEnd);
                }

                var c = input[pos];

                if (c != ' ')
                {
                    break;
                }

                pos++;

                if (context.EmailAddressExtractor.IsOutOfCapacity(pos))
                {
                    segment = default;
                    return new TextDataExtractionResult(
                        context.EmailAddressExtractor.MaxConsumption!.Value + 1 - start,
                        TextDataExtractionErrorCodes.InputIsTooLong);
                }
            }

            var consumed = pos - start;
            segment = new Segment(
                SegmentType.LocalPartSpace,
                start,
                consumed,
                null);

            return new TextDataExtractionResult(consumed, null);
        }
    }
}
