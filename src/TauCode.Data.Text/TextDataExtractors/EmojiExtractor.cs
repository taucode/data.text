using System;
using TauCode.Data.Text.EmojiSupport;

namespace TauCode.Data.Text.TextDataExtractors
{
    // todo_deferred: prohibit setting terminator. (and ut)
    public sealed class EmojiExtractor : TextDataExtractorBase<Emoji>
    {
        public static readonly EmojiExtractor Instance = new EmojiExtractor();

        private EmojiExtractor()
            : base(null, Falser)
        {
        }

        private static bool Falser(ReadOnlySpan<char> input, int index) => false;

        public override TerminatingDelegate Terminator
        {
            get => null;
            set => throw new InvalidOperationException();
        }

        protected override TextDataExtractionResult TryExtractImpl(ReadOnlySpan<char> input, out Emoji value)
        {
            return EmojiHelper.Root.TryExtract(input, out value);
        }
    }
}
