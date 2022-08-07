namespace TauCode.Data.Text.EmailAddressSupport.SegmentExtractors;

internal class LocalPartFoldingWhiteSpaceExtractor : SegmentExtractor
{
    internal override bool Accepts(ReadOnlySpan<char> input, EmailAddressExtractionContext context)
    {
        var c = input[context.Position];
        return c == '\r';
    }

    protected override TextDataExtractionResult ExtractImpl(
        ReadOnlySpan<char> input,
        EmailAddressExtractionContext context,
        out Segment? segment)
    {
        var fwsResult = TrySkipFoldingWhiteSpace(input, context.Position, context.EmailAddressExtractor.MaxConsumption);
        if (fwsResult.ErrorCode.HasValue)
        {
            segment = null;
            return fwsResult;
        }

        segment = new Segment(
            SegmentType.LocalPartFoldingWhiteSpace,
            context.Position,
            fwsResult.CharsConsumed,
            null);

        return fwsResult;
    }
}