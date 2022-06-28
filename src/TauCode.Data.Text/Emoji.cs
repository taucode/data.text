using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TauCode.Data.Text.EmojiSupport;
using TauCode.Data.Text.Exceptions;

namespace TauCode.Data.Text
{
    [DebuggerDisplay("{Value}")]
    public readonly struct Emoji : IEquatable<Emoji>, IComparable<Emoji>
    {
        #region Data

        public readonly string Value;

        public readonly string Name;

        #endregion

        #region ctor

        // todo: internal
        public Emoji(string value, string name)
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

        public Emoji(string value)
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

        public override bool Equals(object obj)
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

        #region Extraction

        public static int Extract(ReadOnlySpan<char> input, out Emoji? emoji)
        {
            var consumed = TryExtract(input, out emoji, out var exception);
            if (exception != null)
            {
                throw exception;
            }

            return consumed;
        }

        public static int TryExtract(
            ReadOnlySpan<char> input,
            out Emoji? emoji,
            out TextDataExtractionException exception)
        {
            return EmojiHelper.Root.TryExtract(input, out emoji, out exception);
        }

        #endregion

        #region Querying

        public static IReadOnlyList<Emoji> EnumerateAll()
        {
            return EmojiHelper.Emojis.Values.ToList();
        }

        #endregion
    }
}
