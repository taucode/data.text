using TauCode.Data.Text.Exceptions;

namespace TauCode.Data.Text.EmailAddressSupport
{
    internal static class EmailAddressExtensions
    {
        internal static bool IsMeaningful(this Segment segment)
        {
            return
                segment.Type == SegmentType.Period ||

                segment.Type == SegmentType.LocalPartWord ||
                segment.Type == SegmentType.LocalPartQuotedString ||

                segment.Type == SegmentType.At ||

                segment.Type == SegmentType.DomainLabel ||
                segment.Type == SegmentType.DomainIPAddress ||

                false;
        }

        internal static void AddSegment(this EmailAddressExtractionContext context, Segment segment)
        {
            if (
                segment.Type == SegmentType.LocalPartWord ||
                segment.Type == SegmentType.LocalPartQuotedString ||
                false)
            {
                context.AddLocalPartSegment(segment);
            }
            else if (segment.Type == SegmentType.Period)
            {
                if (context.AtSymbolIndex.HasValue)
                {
                    context.AddDomainSegment(segment);
                }
                else
                {
                    context.AddLocalPartSegment(segment);
                }
            }
            else if (segment.Type == SegmentType.At)
            {
                context.AtSymbolIndex = segment.Start;
            }
            else if (
                segment.Type == SegmentType.DomainLabel ||
                segment.Type == SegmentType.DomainIPAddress ||
                false)
            {
                context.AddDomainSegment(segment);
            }
            else
            {
                throw new TextDataExtractionException($"Internal error ('{nameof(AddSegment)}').");
            }
        }
    }
}
