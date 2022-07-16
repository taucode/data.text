using System;
using System.Collections.Generic;
using TauCode.Data.Text.Exceptions;
using TauCode.Data.Text.TextDataExtractors;

namespace TauCode.Data.Text.EmailAddressSupport
{
    internal abstract class SegmentExtractor
    {
        private static readonly HashSet<char> ValidFwsSpaces = new HashSet<char>(new[] { ' ', (char)160 });

        internal abstract bool Accepts(ReadOnlySpan<char> input, EmailAddressExtractionContext context);

        internal TextDataExtractionResult Extract(
            ReadOnlySpan<char> input,
            EmailAddressExtractionContext context,
            out Segment? segment)
        {
            if (context.Position >= Helper.Constants.EmailAddress.MaxConsumption || context.Position < 0)
            {
                throw new TextDataExtractionException(
                    "Context position is out of range. See inner exception.",
                    TextDataExtractionErrorCodes.InternalError,
                    context.Position);
            }

            var result = this.ExtractImpl(input, context, out segment);
            return result;
        }

        protected abstract TextDataExtractionResult ExtractImpl(
            ReadOnlySpan<char> input,
            EmailAddressExtractionContext context,
            out Segment? segment);

        protected static TextDataExtractionResult TrySkipEmoji(
            ReadOnlySpan<char> emojiSpan)
        {
            var emojiResult = EmojiExtractor.Instance.TryExtract(emojiSpan, out var emoji);
            var c = emojiSpan[0];

            switch (emojiResult.ErrorCode)
            {
                case null:
                    return new TextDataExtractionResult(emojiResult.CharsConsumed, null);

                case TextDataExtractionErrorCodes.NonEmojiCharacter:
                    switch (emojiResult.CharsConsumed)
                    {
                        // 'case 0:' is impossible since this method callers should check that 0th char of the span is emoji start.

                        case 1:
                            if (
                                c.IsDecimalDigit() ||
                                EmailAddressExtractor.AllowedSymbols.Contains(c) ||
                                false)
                            {
                                // something like #, *, 0..9
                                return new TextDataExtractionResult(1, null);

                            }
                            else
                            {
                                return emojiResult;
                            }

                        default:
                            return emojiResult;
                    }

                case TextDataExtractionErrorCodes.UnexpectedEnd:
                    return emojiResult;

                default:
                    // should never happen
                    return new TextDataExtractionResult(
                        emojiResult.CharsConsumed,
                        TextDataExtractionErrorCodes.InternalError);
            }
        }

        protected static TextDataExtractionResult TrySkipFoldingWhiteSpace(
            ReadOnlySpan<char> input,
            int position)
        {
            var length = input.Length;
            var start = position;
            var pos = start + 1;  // skip '\r' since we've got here

            for (var i = 1; i < Helper.Constants.EmailAddress.FoldingWhiteSpaceLength; i++) // 'i = 1' because we skipped '\r' since we've got here
            {
                pos = start + i;
                if (pos >= Helper.Constants.EmailAddress.MaxConsumption)
                {
                    return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.InputTooLong);
                }

                if (pos == length)
                {
                    // unclosed fws
                    return new TextDataExtractionResult(pos - start, TextDataExtractionErrorCodes.UnexpectedEnd);
                }

                var c = input[pos];

                var fwsOk = IsValidFwsChar(c, i);
                if (!fwsOk)
                {
                    return new TextDataExtractionResult(pos - start, TextDataExtractionErrorCodes.UnexpectedCharacter);
                }
            }

            var consumed = pos - start;
            return new TextDataExtractionResult(consumed, null);
        }

        private static bool IsValidFwsChar(char c, int pos)
        {
            switch (pos)
            {
                case 0: return c == '\r';
                case 1: return c == '\n';
                case 2: return ValidFwsSpaces.Contains(c);

                default:
                    throw new ArgumentOutOfRangeException(nameof(pos));
            }
        }
    }
}
