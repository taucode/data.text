using System;
using System.Collections.Generic;

namespace TauCode.Data.Text.TextDataExtractors
{
    public class ItemExtractor<T> : TextDataExtractorBase<T>
    {
        private readonly Func<string, T> _parser;
        private readonly StringItemExtractor _innerExtractor;

        public ItemExtractor(
            HashSet<string> items,
            bool ignoreCase,
            Func<string, T> parser,
            TerminatingDelegate terminator = null)
            : base(
                null,
                terminator)
        {
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
            _innerExtractor = new StringItemExtractor(items, ignoreCase, terminator);
        }

        protected override TextDataExtractionResult TryExtractImpl(
            ReadOnlySpan<char> input,
            out T value)
        {
            var innerResult = _innerExtractor.TryExtract(input, out var stringValue);
            if (innerResult.ErrorCode.HasValue)
            {
                value = default;
                return innerResult;
            }

            value = _parser(stringValue);
            return new TextDataExtractionResult(innerResult.CharsConsumed, null);
        }
    }
}
