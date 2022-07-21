using System;
using TauCode.Data.Text.TextDataExtractors;

namespace TauCode.Data.Text.EmailAddressSupport.SegmentExtractors
{
    internal class TerminatingSegmentExtractor : SegmentExtractor
    {
        private readonly EmailAddressExtractor _emailAddressExtractor;

        internal TerminatingSegmentExtractor(EmailAddressExtractor emailAddressExtractor)
        {
            _emailAddressExtractor = emailAddressExtractor;
        }

        internal override bool Accepts(ReadOnlySpan<char> input, EmailAddressExtractionContext context)
        {
            return _emailAddressExtractor.IsTermination(input, context.Position);
        }

        protected override TextDataExtractionResult ExtractImpl(
            ReadOnlySpan<char> input,
            EmailAddressExtractionContext context,
            out Segment? segment)
        {
            segment = new Segment(SegmentType.Termination, context.Position, 1, null);
            return new TextDataExtractionResult(0, null);
        }
    }
}
