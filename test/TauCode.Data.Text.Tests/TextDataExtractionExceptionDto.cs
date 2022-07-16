using System;

namespace TauCode.Data.Text.Tests;

public class TextDataExtractionExceptionDto : IEquatable<TextDataExtractionExceptionDto>
{
    public string Message { get; set; }
    public int ErrorCode { get; set; }
    public int CharsConsumed { get; set; }

    public bool Equals(TextDataExtractionExceptionDto other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return
            Message == other.Message &&
            ErrorCode == other.ErrorCode &&
            CharsConsumed == other.CharsConsumed;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((TextDataExtractionExceptionDto)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Message, ErrorCode, CharsConsumed);
    }
}
