using System;
using TauCode.Data.Text.TextDataExtractors;

namespace TauCode.Data.Text.EmailAddressSupport.SegmentExtractors
{
    internal class TerminatingSegmentExtractor : SegmentExtractor
    {
        private readonly EmailAddressExtractor _host;

        internal TerminatingSegmentExtractor(EmailAddressExtractor host)
        {
            _host = host;
        }

        internal override bool Accepts(ReadOnlySpan<char> input, EmailAddressExtractionContext context)
        {
            return _host.Terminator(input, context.Position);
        }

        protected override TextDataExtractionResult ExtractImpl(
            ReadOnlySpan<char> input,
            EmailAddressExtractionContext context,
            out Segment? segment)
        {
            throw new InvalidOperationException();
        }
    }
}
