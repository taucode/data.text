using System.Diagnostics;

namespace TauCode.Data.Text.EmailAddressSupport;

[DebuggerDisplay("{Type}")]
internal readonly struct Segment
{
    internal Segment(SegmentType type, int start, int length, HostName? ipHostName)
    {
        this.Type = type;
        this.Start = start;
        this.Length = length;
        this.IPHostName = ipHostName;
    }

    internal readonly SegmentType Type;
    internal readonly int Start;
    internal readonly int Length;
    internal readonly HostName? IPHostName;
}