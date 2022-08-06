using System.Runtime.Serialization;

namespace TauCode.Data.Text.Exceptions;

[Serializable]
public class TextDataExtractionException : Exception
{
    internal TextDataExtractionException(string message)
        : base(message)
    {
    }

    internal TextDataExtractionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public TextDataExtractionException(string message, int errorCode, int charsConsumed)
        : base(message)
    {
        this.ErrorCode = errorCode;
        this.CharsConsumed = charsConsumed;
    }

    public TextDataExtractionException(string message, int errorCode, int charsConsumed, Exception inner)
        : base(message, inner)
    {
        this.ErrorCode = errorCode;
        this.CharsConsumed = charsConsumed;
    }

    protected TextDataExtractionException(
        SerializationInfo info,
        StreamingContext context) : base(info, context)
    {
    }

    public int ErrorCode { get; }
    public int CharsConsumed { get; }
}