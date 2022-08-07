using System.Diagnostics;
using TauCode.Data.Text.EmojiSupport;
using TauCode.Data.Text.Exceptions;
using TauCode.Data.Text.TextDataExtractors;

namespace TauCode.Data.Text;

[DebuggerDisplay("{Value}")]
public readonly struct Emoji : IEquatable<Emoji>, IComparable<Emoji>
{
    #region Data

    public readonly string Value;
    public readonly string Name;

    #endregion

    #region ctor

    internal Emoji(string value, string name)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        if (value.Length == 0)
        {
            throw new ArgumentException("Emoji value cannot be empty.", nameof(value));
        }

        this.Value = value;
        this.Name = name;
    }

    internal Emoji(string value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        if (value.Length == 0)
        {
            throw new ArgumentException("Emoji value cannot be empty.", nameof(value));
        }

        var exists = EmojiHelper.Emojis.TryGetValue(value, out var realEmoji);
        if (exists)
        {
            this.Value = realEmoji.Value;
            this.Name = realEmoji.Name;
        }

        throw new ArgumentException("Invalid emoji value.", nameof(value));
    }

    #endregion

    #region IEquatable<Emoji> Members

    public bool Equals(Emoji other)
    {
        if (this.Value == null)
        {
            return other.Value == null;
        }

        return this.Value.Equals(other.Value, StringComparison.Ordinal);
    }

    #endregion

    #region IComparable<Emoji> Members

    public int CompareTo(Emoji other)
    {
        return string.CompareOrdinal(this.Value, other.Value);
    }

    #endregion

    #region Overridden

    public override bool Equals(object? obj)
    {
        if (obj is Emoji other)
        {
            return this.Equals(other);
        }

        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.Value, this.Name);
    }

    public override string ToString() => this.Value;

    #endregion

    #region Parsing

    public static Emoji Parse(
        ReadOnlySpan<char> input)
    {
        var result = EmojiExtractor.Instance.TryExtract(input, out var value);
        if (result.ErrorCode.HasValue)
        {
            var message = EmojiExtractor.Instance.GetErrorMessage(result.ErrorCode.Value);
            throw new TextDataExtractionException(message, result.ErrorCode.Value, result.CharsConsumed);
        }

        if (result.CharsConsumed != input.Length)
        {
            var errorCode = TextDataExtractionErrorCodes.UnexpectedCharacter;
            var message = EmojiExtractor.Instance.GetErrorMessage(errorCode);

            throw new TextDataExtractionException(message, errorCode, result.CharsConsumed);
        }

        return value;
    }

    public static bool TryParse(
        ReadOnlySpan<char> input,
        out Emoji emoji)
    {
        var result = EmojiExtractor.Instance.TryExtract(input, out emoji);

        if (result.ErrorCode != null)
        {
            return false;
        }

        if (result.CharsConsumed != input.Length)
        {
            emoji = default;
            return false;
        }

        return true;
    }

    #endregion

    #region Extraction

    //public static int Extract(ReadOnlySpan<char> input, out Emoji? emoji)
    //{
    //    var consumed = TryExtract(input, out emoji, out var exception);
    //    if (exception != null)
    //    {
    //        throw exception;
    //    }

    //    return consumed;
    //}

    //public static int TryExtract(
    //    ReadOnlySpan<char> input,
    //    out Emoji? emoji,
    //    out TextDataExtractionException exception)
    //{
    //    return EmojiHelper.Root.TryExtract(input, out emoji, out exception);
    //}

    #endregion

    #region Querying

    public static IReadOnlyList<Emoji> EnumerateAll()
    {
        return EmojiHelper.Emojis.Values.ToList();
    }

    #endregion
}