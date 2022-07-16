using System;
using System.Collections.Generic;
using System.Linq;

namespace TauCode.Data.Text.TextDataExtractors
{
    public class StringItemExtractor : TextDataExtractorBase<string>
    {
        public StringItemExtractor(
            HashSet<string> items,
            bool ignoreCase,
            TerminatingDelegate terminator = null)
            : base(
                null,
                terminator)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            if (items.Count == 0 || items.Any(string.IsNullOrEmpty))
            {
                throw new ArgumentException($"'{nameof(items)}' cannot be empty or contain empty items.", nameof(items));
            }

            this.Items = items;
            this.IgnoreCase = ignoreCase;
        }

        public HashSet<string> Items { get; }

        public bool IgnoreCase { get; }

        protected override TextDataExtractionResult TryExtractImpl(
            ReadOnlySpan<char> input,
            out string value)
        {
            var comparison = this.IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            foreach (var item in Items)
            {
                if (input.StartsWith(item, comparison))
                {
                    if (input.Length == item.Length)
                    {
                        value = item;
                        return new TextDataExtractionResult(item.Length, null);
                    }
                    else
                    {
                        if (this.Terminator(input, item.Length))
                        {
                            value = item;
                            return new TextDataExtractionResult(item.Length, null);
                        }

                        value = default;
                        return new TextDataExtractionResult(item.Length, TextDataExtractionErrorCodes.ItemNotFound);
                    }
                }
            }

            value = default;
            return new TextDataExtractionResult(0, TextDataExtractionErrorCodes.ItemNotFound); // todo_deferred ut
        }
    }
}
