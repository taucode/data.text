using System;

namespace TauCode.Data.Text.Tests;

public class EmojiDto : IEquatable<EmojiDto>
{
    public string Value { get; set; }
    public string Name { get; set; }

    public bool Equals(EmojiDto other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Value == other.Value && Name == other.Name;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((EmojiDto)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Value, Name);
    }
}
