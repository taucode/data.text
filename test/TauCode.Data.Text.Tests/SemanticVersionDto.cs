using System;

namespace TauCode.Data.Text.Tests;

public class SemanticVersionDto : IEquatable<SemanticVersionDto>
{
    public int Major { get; set; }
    public int Minor { get; set; }
    public int Patch { get; set; }
    public string PreRelease { get; set; }
    public string BuildMetadata { get; set; }

    public bool Equals(SemanticVersionDto other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return
            Major == other.Major &&
            Minor == other.Minor &&
            Patch == other.Patch &&
            PreRelease == other.PreRelease;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((SemanticVersionDto)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Major, Minor, Patch, PreRelease);
    }
}
