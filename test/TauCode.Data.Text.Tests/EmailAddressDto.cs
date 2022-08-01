namespace TauCode.Data.Text.Tests;

public class EmailAddressDto : IEquatable<EmailAddressDto>
{
    public string LocalPart { get; set; } = default!; // deserialized from JSON
    public HostNameDto Domain { get; set; } = default!; // deserialized from JSON

    public bool Equals(EmailAddressDto? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return LocalPart == other.LocalPart && Equals(Domain, other.Domain);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((EmailAddressDto)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(LocalPart, Domain);
    }
}
