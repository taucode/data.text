using System.ComponentModel;

namespace TauCode.Data.Text
{
    public readonly ref struct TextDataExtractionResult
    {
        public TextDataExtractionResult(int charsConsumed, int? errorCode)
        {
            if (charsConsumed < 0)
            {
                throw new InvalidEnumArgumentException(nameof(charsConsumed));
            }

            this.CharsConsumed = charsConsumed;
            this.ErrorCode = errorCode;
        }

        public readonly int CharsConsumed;
        public readonly int? ErrorCode;
    }
}
