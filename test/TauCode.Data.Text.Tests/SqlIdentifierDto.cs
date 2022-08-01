namespace TauCode.Data.Text.Tests;

public class SqlIdentifierDto : IEquatable<SqlIdentifierDto>
{
    public string Value { get; set; } = null!; // deserialized from JSON
    public SqlIdentifierDelimiter Delimiter { get; set; }

    public bool Equals(SqlIdentifierDto? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Value == other.Value && Delimiter == other.Delimiter;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((SqlIdentifierDto)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Value, (int)Delimiter);
    }
}