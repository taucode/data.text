using System;
using System.Collections.Generic;

namespace TauCode.Data.Text.TextDataExtractors
{
    public class ItemExtractor<T> : TextDataExtractorBase<T>
    {
        private readonly StringItemExtractor _innerExtractor;

        public ItemExtractor(
            IEnumerable<string> items,
            bool ignoreCase,
            Func<string, bool, T> itemParser,
            TerminatingDelegate terminator = null)
            : base(
                null,
                terminator)
        {
            ItemParser = itemParser ?? throw new ArgumentNullException(nameof(itemParser));
            _innerExtractor = new StringItemExtractor(items, ignoreCase, terminator);
        }

        public override int? MaxConsumption
        {
            get => _innerExtractor.MaxConsumption;
            set => _innerExtractor.MaxConsumption = value;
        }

        public HashSet<string> Items => _innerExtractor.Items;

        public bool IgnoreCase => _innerExtractor.IgnoreCase;

        public Func<string, bool, T> ItemParser { get; }

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

            value = ItemParser(stringValue, this.IgnoreCase);
            return new TextDataExtractionResult(innerResult.CharsConsumed, null);
        }
    }
}
