using TauCode.Data.Text.EmojiSupport;
using TauCode.Data.Text.Exceptions;

namespace TauCode.Data.Text.TextDataExtractors
{
    public sealed class EmojiExtractor : TextDataExtractorBase<Emoji>
    {
        public static readonly EmojiExtractor Instance = new EmojiExtractor();

        private EmojiExtractor()
            : base(null, null)
        {
        }

        public override int? MaxConsumption
        {
            get => null;
            set => throw new InvalidOperationException();
        }

        public override TerminatingDelegate? Terminator
        {
            get => null;
            set => throw new InvalidOperationException();
        }

        public override bool TryParse(ReadOnlySpan<char> input, out Emoji value)
        {
            var result = this.TryExtract(input, out value);
            return result.ErrorCode == null;
        }

        public override Emoji Parse(ReadOnlySpan<char> input)
        {
            var result = this.TryExtract(input, out var value);

            if (result.ErrorCode == null)
            {
                return value;
            }

            var message = Helper.GetErrorMessage(result.ErrorCode.Value);

            throw new TextDataExtractionException(message, result.ErrorCode.Value, result.CharsConsumed);
        }

        protected override TextDataExtractionResult TryExtractImpl(ReadOnlySpan<char> input, out Emoji value)
        {
            return EmojiHelper.Root.TryExtract(input, out value);
        }
    }
}
