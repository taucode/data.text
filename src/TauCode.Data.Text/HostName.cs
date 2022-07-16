using System;
using TauCode.Data.Text.Exceptions;
using TauCode.Data.Text.TextDataExtractors;

namespace TauCode.Data.Text
{
    public readonly struct HostName : IEquatable<HostName>
    {
        #region Static

        private static readonly HostNameExtractor Extractor = new HostNameExtractor((input, index) => false);

        #endregion

        #region Fields

        public readonly HostNameKind Kind;
        public readonly string Value;

        #endregion

        #region ctor

        internal HostName(HostNameKind kind, string value)
        {
            this.Kind = kind;
            this.Value = value;
        }

        #endregion

        #region Parsing

        public static HostName Parse(
            ReadOnlySpan<char> input)
        {
            var result = Extractor.TryExtract(input, out var value);
            if (result.ErrorCode.HasValue)
            {
                var message = Extractor.GetErrorMessage(result.ErrorCode.Value);
                throw new TextDataExtractionException(message, result.ErrorCode.Value, result.CharsConsumed);
            }

            return value;
        }

        public static bool TryParse(
            ReadOnlySpan<char> input,
            out HostName hostName)
        {
            var result = Extractor.TryExtract(input, out hostName);
            return result.ErrorCode == null;
        }

        #endregion

        #region IEquatable<HostName> Members

        public bool Equals(HostName other)
        {
            return
                this.Kind == other.Kind &&
                this.Value == other.Value;
        }

        #endregion

        #region Overridden

        public override bool Equals(object obj)
        {
            return obj is HostName other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int)this.Kind, this.Value);
        }

        public override string ToString() => this.Value;

        #endregion
    }
}
