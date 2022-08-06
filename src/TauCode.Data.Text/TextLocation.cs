namespace TauCode.Data.Text;

public readonly struct TextLocation : IEquatable<TextLocation>, IComparable<TextLocation>
{
    #region Static

    public static readonly TextLocation Zero = new TextLocation(0, 0);

    #endregion

    #region ctor

    public TextLocation(int line, int column)
    {
        if (line < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(line));
        }

        if (column < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(column));
        }

        this.Line = line;
        this.Column = column;
    }

    #endregion

    #region Public

    public int Line { get; }
    public int Column { get; }


    #endregion

    #region IEquatable<TextLocation> Members

    public bool Equals(TextLocation other)
    {
        return
            this.Line == other.Line &&
            this.Column == other.Column;
    }

    #endregion

    #region IComparable<TextLocation> Members

    public int CompareTo(TextLocation other)
    {
        var lineComparison = this.Line.CompareTo(other.Line);
        if (lineComparison != 0)
        {
            return lineComparison;
        }

        return this.Column.CompareTo(other.Column);
    }

    #endregion

    #region Overridden

    public override bool Equals(object? obj)
    {
        return
            obj is TextLocation other &&
            this.Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Line, Column);
    }

    public override string ToString() => $"Line: {this.Line} Column: {this.Column}";


    #endregion

    #region Operators

    public static bool operator ==(TextLocation a, TextLocation b) => a.Equals(b);

    public static bool operator !=(TextLocation a, TextLocation b) => !a.Equals(b);

    public static bool operator <(TextLocation a, TextLocation b) => a.CompareTo(b) > 0;

    public static bool operator >(TextLocation a, TextLocation b) => a.CompareTo(b) < 0;

    public static bool operator >=(TextLocation a, TextLocation b) => a.CompareTo(b) >= 0;

    public static bool operator <=(TextLocation a, TextLocation b) => a.CompareTo(b) <= 0;


    #endregion
}