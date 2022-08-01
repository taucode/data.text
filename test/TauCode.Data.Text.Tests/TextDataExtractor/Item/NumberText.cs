namespace TauCode.Data.Text.Tests.TextDataExtractor.Item;

public class NumberText : IEquatable<NumberText>
{
    public static readonly NumberText Zero = new NumberText("Zero", 0);
    public static readonly NumberText One = new NumberText("One", 1);
    public static readonly NumberText Two = new NumberText("Two", 2);
    public static readonly NumberText Three = new NumberText("Three", 3);

    /// <summary>
    /// Deserialization ctor
    /// </summary>
    public NumberText()
    {
        this.Text = default!; // deserialized from JSON
    }

    public NumberText(string text, int number)
    {
        this.Text = text;
        this.Number = number;
    }

    public string Text { get; set; }
    public int Number { get; set; }

    public bool Equals(NumberText? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Text == other.Text && Number == other.Number;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((NumberText)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Text, Number);
    }

    public static NumberText Parse(string arg, bool ignoreCase)
    {
        var comparisonType = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

        foreach (var numberText in new[] { Zero, One, Two, Three })
        {
            if (numberText.Text.Equals(arg, comparisonType))
            {
                return numberText;
            }
        }

        return null!;
    }
}
