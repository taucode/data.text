﻿using System;

namespace TauCode.Data.Text.EmailAddressSupport.SegmentExtractors
{
    internal class AtSignExtractor : SegmentExtractor
    {
        internal override bool Accepts(ReadOnlySpan<char> input, EmailAddressExtractionContext context)
        {
            var c = input[context.Position];
            return c == '@';
        }

        protected override TextDataExtractionResult ExtractImpl(
            ReadOnlySpan<char> input,
            EmailAddressExtractionContext context,
            out Segment? segment)
        {
            // input[context.Position] MUST be '@'

            segment = new Segment(SegmentType.At, context.Position, 1, null);
            return new TextDataExtractionResult(1, null);
        }
    }
}
