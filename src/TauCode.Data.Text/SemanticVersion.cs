using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TauCode.Data.Text.Exceptions;
using TauCode.Data.Text.SemanticVersionSupport;

// todo clean
namespace TauCode.Data.Text
{
    public class SemanticVersion : IComparable<SemanticVersion>, IEquatable<SemanticVersion>
    {
        #region Constants & Invariants

        private const string Pattern = @"^(?<major>0|[1-9]\d*)\.(?<minor>0|[1-9]\d*)\.(?<patch>0|[1-9]\d*)(?:-(?<preRelease>(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+(?<buildMetadata>[0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$";

        private const int MaxLength = 256;
        private static readonly HashSet<char> AcceptableChars;
        //private static readonly HashSet<char> AcceptableTerminatingChars;
        private static readonly HashSet<char> EmptyCharSet = new HashSet<char>();

        #endregion

        #region Static ctor

        static SemanticVersion()
        {
            #region AcceptableChars

            var chars = new List<char> { '+', '-', '.' };

            for (var c = 'a'; c <= 'z'; c++)
            {
                chars.Add(c);
            }

            for (var c = 'A'; c <= 'Z'; c++)
            {
                chars.Add(c);
            }

            for (var c = '0'; c <= '9'; c++)
            {
                chars.Add(c);
            }

            AcceptableChars = new HashSet<char>(chars);


            #endregion

            #region AcceptableTerminatingChars

            //var list = new List<char>
            //{
            //    '\r',
            //    '\n',
            //    '\t',
            //    '\v',
            //    '\f',
            //    ' ',
            //    '~',
            //    '`',
            //    '!',
            //    '@',
            //    '#',
            //    '$',
            //    '%',
            //    '^',
            //    '&',
            //    '*',
            //    '(',
            //    ')',
            //    '=',
            //    '\'',
            //    '"',
            //    '[',
            //    ']',
            //    '{',
            //    '}',
            //    '|',
            //    '/',
            //    '\\',
            //    ',',
            //    '?',
            //    '<',
            //    '>',
            //    ';',
            //};

            //AcceptableTerminatingChars = new HashSet<char>(list);

            #endregion
        }

        #endregion

        #region Fields

        public readonly int Major;
        public readonly int Minor;
        public readonly int Patch;
        public readonly string PreRelease;
        public readonly string BuildMetadata;

        private List<SemanticVersionIdentifier> _preReleaseIdentifiers;
        private List<SemanticVersionIdentifier> _buildMetadataIdentifiers;

        #endregion

        #region ctor

        private SemanticVersion(
            int major,
            int minor,
            int patch,
            string preRelease,
            string buildMetadata)
        {
            this.Major = major;
            this.Minor = minor;
            this.Patch = patch;
            this.PreRelease = preRelease;
            this.BuildMetadata = buildMetadata;
        }

        #endregion

        #region Private

        private List<SemanticVersionIdentifier> PreReleaseIdentifiers
        {
            get
            {
                if (_preReleaseIdentifiers == null)
                {
                    if (this.PreRelease == null)
                    {
                        _preReleaseIdentifiers = new List<SemanticVersionIdentifier>();
                    }
                    else
                    {
                        _preReleaseIdentifiers = ResolveMetadataIdentifiers(this.PreRelease);
                    }
                }

                return _preReleaseIdentifiers;
            }
        }

        private List<SemanticVersionIdentifier> BuildMetadataIdentifiers
        {
            get
            {
                if (_buildMetadataIdentifiers == null)
                {
                    if (this.BuildMetadata == null)
                    {
                        _buildMetadataIdentifiers = new List<SemanticVersionIdentifier>();
                    }
                    else
                    {
                        _buildMetadataIdentifiers = ResolveMetadataIdentifiers(this.BuildMetadata);
                    }
                }

                return _buildMetadataIdentifiers;
            }
        }

        private static List<SemanticVersionIdentifier> ResolveMetadataIdentifiers(string value)
        {
            return value
                .Split('.')
                .Select(x => new SemanticVersionIdentifier(x))
                .ToList();
        }

        #endregion

        #region Custom ToString()

        public string ToString(bool includeBuildMetadata)
        {
            var sb = new StringBuilder();

            sb.Append(this.Major);
            sb.Append('.');
            sb.Append(this.Minor);
            sb.Append('.');
            sb.Append(this.Patch);

            if (this.PreRelease != null)
            {
                sb.Append('-');
                sb.Append(this.PreRelease);
            }

            if (includeBuildMetadata && this.BuildMetadata != null)
            {
                sb.Append('+');
                sb.Append(this.BuildMetadata);
            }

            var res = sb.ToString();
            return res;
        }

        #endregion

        #region Overridden

        public override string ToString() => this.ToString(true);

        public override bool Equals(object obj)
        {
            return obj is SemanticVersion other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                this.Major,
                this.Minor,
                this.Patch,
                this.PreRelease);
        }


        #endregion

        #region IEquatable<SemanticVersion> Members

        public bool Equals(SemanticVersion other)
        {
            if (other == null)
            {
                return false;
            }

            return
                this.Major == other.Major &&
                this.Minor == other.Minor &&
                this.Patch == other.Patch &&
                this.PreRelease == other.PreRelease;
        }

        #endregion

        #region IComparable<SemanticVersion2> Members

        public int CompareTo(SemanticVersion other)
        {
            var majorComparison = this.Major.CompareTo(other.Major);
            if (majorComparison != 0)
            {
                return majorComparison;
            }

            var minorComparison = this.Minor.CompareTo(other.Minor);
            if (minorComparison != 0)
            {
                return minorComparison;
            }

            var patchComparison = this.Patch.CompareTo(other.Patch);
            if (patchComparison != 0)
            {
                return patchComparison;
            }

            if (this.PreRelease == null)
            {
                // 'this' is a release version
                if (other.PreRelease == null)
                {
                    return 0;
                }

                return 1; // 'this' is a release, 'other' is pre-release => 'this' is bigger
            }

            if (other.PreRelease == null)
            {
                // 'other' is a release version
                if (this.PreRelease == null)
                {
                    return 0;
                }

                return -1; // 'this' is a pre-release, 'other' is release => 'this' is smaller
            }

            var minLength = Math.Min(this.PreReleaseIdentifiers.Count, other.PreReleaseIdentifiers.Count);

            for (var i = 0; i < minLength; i++)
            {
                var thisIdentifier = this.PreReleaseIdentifiers[i];
                var otherIdentifier = other.PreReleaseIdentifiers[i];

                var identifierComparison = thisIdentifier.CompareTo(otherIdentifier);
                if (identifierComparison != 0)
                {
                    return identifierComparison;
                }
            }

            return this.PreReleaseIdentifiers.Count.CompareTo(other.PreReleaseIdentifiers.Count);
        }

        #endregion

        #region Operators

        public static bool operator ==(SemanticVersion v1, SemanticVersion v2)
        {
            if (ReferenceEquals(v1, null))
            {
                return ReferenceEquals(v2, null);
            }

            return v1.Equals(v2);
        }

        public static bool operator !=(SemanticVersion v1, SemanticVersion v2)
        {
            return !(v1 == v2);
        }

        public static bool operator <(SemanticVersion v1, SemanticVersion v2)
        {
            return v1.CompareTo(v2) < 0;
        }

        public static bool operator >(SemanticVersion v1, SemanticVersion v2)
        {
            return v1.CompareTo(v2) > 0;
        }

        public static bool operator <=(SemanticVersion v1, SemanticVersion v2)
        {
            return !(v1 > v2);
        }

        public static bool operator >=(SemanticVersion v1, SemanticVersion v2)
        {
            return !(v1 < v2);
        }

        #endregion

        #region Parsing

        public static SemanticVersion Parse(ReadOnlySpan<char> input)
        {
            Extract(input, out var semanticVersion, (span, position) => false);
            return semanticVersion;
        }

        #endregion

        #region Extraction

        public static int Extract(
            ReadOnlySpan<char> input,
            out SemanticVersion semanticVersion,
            TerminatingDelegate terminatingPredicate = null)
        {
            var result = TryExtract(
                input,
                out semanticVersion,
                out var exception,
                terminatingPredicate);

            if (exception != null)
            {
                throw exception;
            }

            return result;
        }

        public static int TryExtract(
            ReadOnlySpan<char> input,
            out SemanticVersion semanticVersion,
            out TextDataExtractionException exception,
            TerminatingDelegate terminatingPredicate)
        {
            // todo check terminatingChars
            // todo ut terminatingChars

            terminatingPredicate ??= Helper.DefaultTerminatingPredicate;

            int pos;

            for (pos = 0; pos < input.Length; pos++)
            {
                var c = input[pos];

                if (terminatingPredicate(input, pos))
                {
                    break;
                }

                if (AcceptableChars.Contains(c))
                {
                    continue;
                }


                semanticVersion = null;
                exception = Helper.CreateException(ExtractionErrorTag.UnexpectedChar, pos);

                return 0;
            }

            if (pos == 0)
            {
                semanticVersion = null;
                exception = Helper.CreateException(ExtractionErrorTag.EmptyInput, null);
                return 0;
            }

            var stringInput = input[..pos].ToString(); // bad (performance), but let it be.

            var match = Regex.Match(stringInput, Pattern);
            if (!match.Success)
            {
                semanticVersion = null;
                exception = Helper.CreateException(ExtractionErrorTag.InvalidSemanticVersion, 0);
                return 0;
            }

            int major;
            int minor;
            int patch;

            try
            {
                // not great performance
                var majorString = match.Groups["major"].Value;
                major = int.Parse(majorString);

                var minorString = match.Groups["minor"].Value;
                minor = int.Parse(minorString);

                var patchString = match.Groups["patch"].Value;
                patch = int.Parse(patchString);
            }
            catch
            {
                semanticVersion = null;
                exception = Helper.CreateException(ExtractionErrorTag.InvalidSemanticVersion, 0);
                return 0;
            }

            var preRelease = match.Groups["preRelease"].Value;
            if (preRelease == string.Empty)
            {
                preRelease = null;
            }

            var buildMetadata = match.Groups["buildMetadata"].Value;
            if (buildMetadata == string.Empty)
            {
                buildMetadata = null;
            }

            semanticVersion = new SemanticVersion(major, minor, patch, preRelease, buildMetadata);
            exception = null;
            return match.Length;
        }

        #endregion
    }
}
