using System;
using System.Linq;

namespace TauCode.Data.Text.SemanticVersionSupport
{
    // todo: rename to identifier
    internal readonly struct SemanticVersionIdentifier : IComparable<SemanticVersionIdentifier>
    {
        internal SemanticVersionIdentifier(string value)
        {
            this.Type = DetectType(value);
            this.Value = value;
        }

        private static SemanticVersionIdentifierType DetectType(string value)
        {
            if (value.All(x => x.IsDecimalDigit()))
            {
                return SemanticVersionIdentifierType.Numeric;
            }

            return SemanticVersionIdentifierType.Text;
        }

        internal SemanticVersionIdentifierType Type { get; }

        internal string Value { get; }

        public int CompareTo(SemanticVersionIdentifier other)
        {
            if (this.Type == SemanticVersionIdentifierType.Text && other.Type == SemanticVersionIdentifierType.Text)
            {
                return string.CompareOrdinal(this.Value, other.Value);
            }

            if (this.Type == SemanticVersionIdentifierType.Numeric && other.Type == SemanticVersionIdentifierType.Numeric)
            {
                return CompareAsNumeric(this.Value, other.Value);
            }

            return this.Type.CompareTo(other.Type);
        }

        private int CompareAsNumeric(string s1, string s2)
        {
            var compareLengths = s1.Length.CompareTo(s2.Length);
            if (compareLengths != 0)
            {
                return compareLengths;
            }

            return string.CompareOrdinal(s1, s2);
        }
    }
}
