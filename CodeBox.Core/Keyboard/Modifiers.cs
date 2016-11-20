using System;

namespace CodeBox.Core.Keyboard
{
    [Flags]
    public enum Modifiers
    {
        None = 0,
        Ctrl = 0x01,
        Shift = 0x02,
        Alt = 0x04,
        Cmd = 0x08,
        Move = 0x10
    }
}
