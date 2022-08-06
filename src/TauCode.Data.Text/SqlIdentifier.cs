using System.Text;

namespace TauCode.Data.Text;

public readonly struct SqlIdentifier : IEquatable<SqlIdentifier>
{
    internal SqlIdentifier(string value, SqlIdentifierDelimiter delimiter)
    {
        this.Value = value ?? throw new ArgumentNullException(nameof(value));
        this.Delimiter = delimiter;
    }

    public readonly string Value;
    public readonly SqlIdentifierDelimiter Delimiter;

    public bool Equals(SqlIdentifier other)
    {
        return Value == other.Value && Delimiter == other.Delimiter;
    }

    public override bool Equals(object? obj)
    {
        return obj is SqlIdentifier other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Value, (int)Delimiter);
    }

    public override string? ToString()
    {
        if (this.Value == null)
        {
            return null; // for default value only
        }

        var sb = new StringBuilder();
        var c = this.Delimiter.ToOpeningDelimiter();
        if (c.HasValue)
        {
            sb.Append(c.Value);
        }

        sb.Append(this.Value);

        c = this.Delimiter.ToClosingDelimiter();
        if (c.HasValue)
        {
            sb.Append(c.Value);
        }

        return sb.ToString();
    }
}