namespace TauCode.Data.Text.Tests;

public class HostNameDto : IEquatable<HostNameDto>
{
    public HostNameKind Kind { get; set; }
    public string Value { get; set; } = default!; // deserialized from JSON

    public bool Equals(HostNameDto? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Kind == other.Kind && Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((HostNameDto)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((int)Kind, Value);
    }
}
