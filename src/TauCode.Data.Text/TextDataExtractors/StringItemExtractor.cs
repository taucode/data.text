namespace TauCode.Data.Text.TextDataExtractors
{
    public class StringItemExtractor : TextDataExtractorBase<string>
    {
        public StringItemExtractor(
            IEnumerable<string> items,
            bool ignoreCase,
            TerminatingDelegate? terminator = null)
            : base(
                null,
                terminator)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            var itemHashSet = new HashSet<string>(items);

            if (itemHashSet.Count == 0 || itemHashSet.Any(string.IsNullOrEmpty))
            {
                throw new ArgumentException($"'{nameof(items)}' cannot be empty or contain empty items.", nameof(items));
            }

            this.Items = itemHashSet;
            this.IgnoreCase = ignoreCase;
        }

        public HashSet<string> Items { get; }

        public bool IgnoreCase { get; }

        protected override TextDataExtractionResult TryExtractImpl(
            ReadOnlySpan<char> input,
            out string? value)
        {
            var comparison = this.IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            foreach (var item in this.Items)
            {
                if (input.StartsWith(item, comparison))
                {
                    if (input.Length == item.Length)
                    {
                        if (item.Length > this.MaxConsumption)
                        {
                            value = default;
                            return new TextDataExtractionResult(
                                this.MaxConsumption.Value + 1,
                                TextDataExtractionErrorCodes.InputIsTooLong);
                        }

                        value = item;
                        return new TextDataExtractionResult(item.Length, null);
                    }
                    else
                    {
                        if (this.IsTermination(input, item.Length))
                        {
                            if (item.Length > this.MaxConsumption)
                            {
                                value = default;
                                return new TextDataExtractionResult(
                                    this.MaxConsumption.Value + 1,
                                    TextDataExtractionErrorCodes.InputIsTooLong);
                            }

                            value = item;
                            return new TextDataExtractionResult(item.Length, null);
                        }

                        value = default;
                        return new TextDataExtractionResult(item.Length, TextDataExtractionErrorCodes.ItemNotFound);
                    }
                }
            }

            value = default;
            return new TextDataExtractionResult(0, TextDataExtractionErrorCodes.ItemNotFound);
        }
    }
}
