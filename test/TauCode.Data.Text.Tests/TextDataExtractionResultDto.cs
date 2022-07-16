using System;

namespace TauCode.Data.Text.Tests;

public class TextDataExtractionResultDto : IEquatable<TextDataExtractionResultDto>
{
    public int CharsConsumed { get; set; }
    public int? ErrorCode { get; set; }

    public bool Equals(TextDataExtractionResultDto other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return CharsConsumed == other.CharsConsumed && ErrorCode == other.ErrorCode;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((TextDataExtractionResultDto)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(CharsConsumed, ErrorCode);
    }
}
