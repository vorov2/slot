using System;

namespace CodeBox.Commands
{
    [Flags]
    public enum MouseEvents
    {
        None = 0x00,

        Click = 0x01,

        RightClick = 0x02,

        DoubleClick = 0x04,

        Move = 0x08,

        Hover = 0x10
    }
}
