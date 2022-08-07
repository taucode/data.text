using System.Text;
using TauCode.Data.Text.Exceptions;
using TauCode.Data.Text.SemanticVersionSupport;
using TauCode.Data.Text.TextDataExtractors;

namespace TauCode.Data.Text;

public class SemanticVersion : IComparable<SemanticVersion>, IEquatable<SemanticVersion>
{
    #region Static

    private static readonly SemanticVersionExtractor Extractor = new SemanticVersionExtractor((input, index) => false);

    #endregion

    #region Fields

    public readonly int Major;
    public readonly int Minor;
    public readonly int Patch;
    public readonly string? PreRelease;
    public readonly string? BuildMetadata;

    private List<SemanticVersionIdentifier>? _preReleaseIdentifiers;
    private List<SemanticVersionIdentifier>? _buildMetadataIdentifiers;

    #endregion

    #region ctor

    internal SemanticVersion(
        int major,
        int minor,
        int patch,
        string? preRelease,
        string? buildMetadata)
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

    public override bool Equals(object? obj)
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

    public bool Equals(SemanticVersion? other)
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

    public int CompareTo(SemanticVersion? other)
    {
        if (other == null)
        {
            throw new ArgumentNullException(nameof(other));
        }

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

    public static bool operator ==(SemanticVersion? v1, SemanticVersion? v2)
    {
        if (ReferenceEquals(v1, null))
        {
            return ReferenceEquals(v2, null);
        }

        return v1.Equals(v2);
    }

    public static bool operator !=(SemanticVersion? v1, SemanticVersion? v2)
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
        var result = Extractor.TryExtract(input, out var value);
        if (result.ErrorCode.HasValue)
        {
            var message = Extractor.GetErrorMessage(result.ErrorCode.Value);
            throw new TextDataExtractionException(message, result.ErrorCode.Value, result.CharsConsumed);
        }

        return value!;
    }

    public static bool TryParse(
        ReadOnlySpan<char> input,
        out SemanticVersion? semanticVersion)
    {
        var result = Extractor.TryExtract(input, out semanticVersion);
        return result.ErrorCode == null;
    }

    #endregion
}