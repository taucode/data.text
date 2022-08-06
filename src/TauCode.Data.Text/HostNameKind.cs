namespace TauCode.Data.Text;

public enum HostNameKind : byte
{
    Unknown = 0,

    Regular = 1,
    Internationalized = 2,
    IPv4 = 3,
    IPv6 = 4,
}