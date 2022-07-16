using System;
using TauCode.Data.Text.EmojiSupport;
using TauCode.Data.Text.TextDataExtractors;

namespace TauCode.Data.Text.EmailAddressSupport.SegmentExtractors
{
    internal class CommentExtractor : SegmentExtractor
    {
        internal CommentExtractor(bool isLocalPart)
        {
            this.IsLocalPart = isLocalPart;
        }

        internal bool IsLocalPart { get; }

        internal override bool Accepts(ReadOnlySpan<char> input, EmailAddressExtractionContext context)
        {
            var c = input[context.Position];
            return c == '(';
        }

        protected override TextDataExtractionResult ExtractImpl(
            ReadOnlySpan<char> input,
            EmailAddressExtractionContext context,
            out Segment? segment)
        {
            var length = input.Length;
            var start = context.Position;
            var pos = start + 1; // skip '"'

            var depth = 1;
            var escapeMode = false;

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
                if (c == ')')
                {
                    if (escapeMode)
                    {
                        escapeMode = false;
                    }
                    else
                    {
                        if (depth > 1)
                        {
                            depth--;
                        }
                        else
                        {
                            pos++; // todo_deferred: ut the case when pos hits MaxEmailAddressInputLength
                            break;
                        }
                    }
                }
                else if (c == '(')
                {
                    if (escapeMode)
                    {
                        escapeMode = false;
                    }
                    else
                    {
                        depth++;
                    }
                }
                else if (c == '\\')
                {
                    escapeMode = !escapeMode;
                }
                else if (c == '"')
                {
                    // nothing special for a comment.
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
                else if (c == '\r')
                {
                    if (escapeMode)
                    {
                        escapeMode = false;
                    }
                    else
                    {
                        var fwsResult = TrySkipFoldingWhiteSpace(input, pos);
                        if (fwsResult.ErrorCode.HasValue)
                        {
                            segment = null;
                            return fwsResult;
                        }

                        pos += fwsResult.CharsConsumed;
                        continue;
                    }
                }
                else if (
                    EmailAddressExtractor.AllowedSymbolsInComment.Contains(c) ||
                    char.IsLetterOrDigit(c) ||
                    (c >= 128 && c <= 256)) // top-ASCII is ok.
                {
                    // ok
                    escapeMode = false;
                }
                else
                {
                    segment = null;
                    return new TextDataExtractionResult(pos - start, TextDataExtractionErrorCodes.UnexpectedCharacter);
                }

                pos++;
            }

            var consumed = pos - start;
            segment = new Segment(
                SegmentType.Comment,
                start,
                consumed,
                null);
            return new TextDataExtractionResult(consumed, null);
        }
    }
}
