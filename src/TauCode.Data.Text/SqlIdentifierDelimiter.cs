using System;

namespace TauCode.Data.Text
{
    [Flags]
    public enum SqlIdentifierDelimiter
    {
        None = 0x0001, // NOT 0
        Brackets = 0x0002,
        DoubleQuotes = 0x0004,
        BackQuotes = 0x0008,
    }
}
