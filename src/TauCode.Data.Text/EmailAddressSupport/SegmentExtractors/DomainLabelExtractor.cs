﻿namespace TauCode.Data.Text.EmailAddressSupport.SegmentExtractors;

internal class DomainLabelExtractor : SegmentExtractor
{
    internal override bool Accepts(ReadOnlySpan<char> input, EmailAddressExtractionContext context)
    {
        var c = input[context.Position];

        var result =
            c.IsDecimalDigit() ||
            c.IsUnicodeInternal() ||
            char.IsLetter(c) ||
            false;

        return result;
    }

    protected override TextDataExtractionResult ExtractImpl(
        ReadOnlySpan<char> input,
        EmailAddressExtractionContext context,
        out Segment? segment)
    {
        var length = input.Length;
        var start = context.Position;
        var pos = start;

        char? prevChar = null;

        while (true)
        {
            if (pos == length)
            {
                if (prevChar == '-')
                {
                    segment = default;
                    return new TextDataExtractionResult(pos - start, TextDataExtractionErrorCodes.UnexpectedEnd);
                }

                break;
            }

            var c = input[pos];

            if (
                c.IsDecimalDigit() ||
                c.IsUnicodeInternal() ||
                char.IsLetter(c) ||
                false)
            {
                // ok
            }
            else if (c == '.' || c == '(' || context.EmailAddressExtractor.IsTermination(input, pos))
            {
                if (prevChar == '-')
                {
                    segment = null;
                    return new TextDataExtractionResult(
                        pos - start,
                        TextDataExtractionErrorCodes.UnexpectedCharacter);
                }

                break;
            }
            else if (c == '-')
            {
                if (prevChar == '.')
                {
                    segment = null;
                    return new TextDataExtractionResult(
                        pos - start,
                        TextDataExtractionErrorCodes.UnexpectedCharacter);
                }
            }
            else
            {
                segment = default;
                return new TextDataExtractionResult(pos - start, TextDataExtractionErrorCodes.UnexpectedCharacter);
            }

            prevChar = c;
            pos++;

            if (context.EmailAddressExtractor.IsOutOfCapacity(pos))
            {
                segment = default;
                return new TextDataExtractionResult(
                    pos - start,
                    TextDataExtractionErrorCodes.InputIsTooLong);
            }
        }

        var consumed = pos - start;
        segment = new Segment(SegmentType.DomainLabel, start, consumed, null);
        return new TextDataExtractionResult(consumed, null);
    }
}