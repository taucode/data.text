using System;
using TauCode.Data.Text.EmojiSupport;
using TauCode.Data.Text.TextDataExtractors;

namespace TauCode.Data.Text.EmailAddressSupport.SegmentExtractors
{
    internal class LocalPartWordExtractor : SegmentExtractor
    {
        internal override bool Accepts(ReadOnlySpan<char> input, EmailAddressExtractionContext context) =>
            AcceptsChar(input[context.Position]);

        protected override TextDataExtractionResult ExtractImpl(
            ReadOnlySpan<char> input,
            EmailAddressExtractionContext context,
            out Segment? segment)
        {
            var length = input.Length;
            var start = context.Position;
            var pos = start;

            while (true)
            {
                // '>' can be because of emoji thing (Emoji extractor isn't aware of MaxEmailAddressInputLength)
                if (pos >= Helper.Constants.EmailAddress.MaxConsumption)
                {
                    segment = default;
                    return new TextDataExtractionResult(
                        Helper.Constants.EmailAddress.MaxConsumption - start,
                        TextDataExtractionErrorCodes.InputTooLong);
                }

                if (pos == length)
                {
                    break;
                }

                var c = input[pos];
                if (c.IsEmojiStartingChar())
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
                    continue;
                }
                else if (
                    EmailAddressExtractor.AllowedSymbols.Contains(c) ||
                    char.IsLetterOrDigit(c))
                {
                    // ok
                }
                else
                {
                    break;
                }

                pos++;
            }

            var consumed = pos - start;
            segment = new Segment(SegmentType.LocalPartWord, start, consumed, null);
            return new TextDataExtractionResult(consumed, null);
        }

        private bool AcceptsChar(char c)
        {
            var acceptsChar =
                EmailAddressExtractor.AllowedSymbols.Contains(c) ||
                char.IsLetterOrDigit(c) ||
                c.IsEmojiStartingChar();

            return acceptsChar;
        }
    }
}
