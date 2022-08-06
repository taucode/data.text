using TauCode.Data.Text.EmojiSupport;
using TauCode.Data.Text.TextDataExtractors;

namespace TauCode.Data.Text.EmailAddressSupport.SegmentExtractors;

internal class LocalPartQuotedStringExtractor : SegmentExtractor
{
    internal override bool Accepts(ReadOnlySpan<char> input, EmailAddressExtractionContext context)
    {
        var c = input[context.Position];
        return c == '"';
    }

    protected override TextDataExtractionResult ExtractImpl(
        ReadOnlySpan<char> input,
        EmailAddressExtractionContext context,
        out Segment? segment)
    {
        var length = input.Length;
        var start = context.Position;
        var pos = start + 1; // skip '"'

        var escapeMode = false;

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
                segment = null;
                return new TextDataExtractionResult(pos - start, TextDataExtractionErrorCodes.UnclosedString);
            }

            var c = input[pos];
            if (c == '"')
            {
                if (escapeMode)
                {
                    escapeMode = false;
                }
                else
                {
                    pos++;

                    if (context.EmailAddressExtractor.IsOutOfCapacity(pos))
                    {
                        segment = default;
                        return new TextDataExtractionResult(
                            context.EmailAddressExtractor.MaxConsumption!.Value + 1 - start,
                            TextDataExtractionErrorCodes.InputIsTooLong);
                    }

                    break;
                }
            }
            else if (c == '\\')
            {
                escapeMode = !escapeMode;
            }
            else if (c == '\0' || c == '\r') // see RFC: these two must be escaped.
            {
                if (escapeMode)
                {
                    // that's ok.
                }
                else
                {
                    segment = null;
                    return new TextDataExtractionResult(
                        pos - start,
                        TextDataExtractionErrorCodes.UnexpectedCharacter);
                }

                escapeMode = false;
            }
            else if (c.IsEmojiStartingChar())
            {
                var skipEmojiResult = TrySkipEmoji(input[pos..]);
                var skipped = skipEmojiResult.CharsConsumed;

                if (skipEmojiResult.ErrorCode.HasValue)
                {
                    segment = default;
                    return new TextDataExtractionResult(pos + skipped, skipEmojiResult.ErrorCode.Value);
                }

                if (skipEmojiResult.CharsConsumed == 0)
                {
                    // should not happen. ut this!
                    segment = null;
                    return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.InternalError);
                }

                pos += skipEmojiResult.CharsConsumed;
                escapeMode = false;

                continue;
            }
            else if (
                EmailAddressExtractor.AllowedSymbolsInQuotedString.Contains(c) ||
                char.IsLetterOrDigit(c) ||
                (c >= 128 && c <= 256))
            {
                escapeMode = false;
            }
            else
            {
                segment = null;
                return new TextDataExtractionResult(pos - start, TextDataExtractionErrorCodes.UnexpectedCharacter);
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

        if (consumed == 2)
        {
            // empty quoted string
            segment = default;
            return new TextDataExtractionResult(consumed, TextDataExtractionErrorCodes.EmptyString);
        }

        segment = new Segment(
            SegmentType.LocalPartQuotedString,
            start,
            consumed,
            null);

        return new TextDataExtractionResult(consumed, null);
    }
}