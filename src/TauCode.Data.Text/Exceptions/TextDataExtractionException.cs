using System;
using System.Runtime.Serialization;

namespace TauCode.Data.Text.Exceptions
{
    [Serializable]
    public class TextDataExtractionException : Exception
    {
        public TextDataExtractionException()
            : base("Text data extraction failed.")
        {
        }

        public TextDataExtractionException(string message)
            : base(message)
        {
        }

        public TextDataExtractionException(string message, int? index)
            : base(message)
        {
            this.Index = index;
        }

        public TextDataExtractionException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public TextDataExtractionException(string message, int? index, Exception inner)
            : base(message, inner)
        {
            this.Index = index;
        }

        protected TextDataExtractionException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

        public int? Index { get; }

        internal ExtractionErrorTag? ExtractionError;
    }
}
