using System;

namespace CodeBox.Commands
{
    [Flags]
    public enum ActionExponent
    {
        None = 0x00,

        Silent = 0x01,

        RestoreCaret = 0x02,

        Scroll = 0x04,

        SingleRun = 0x08,

        ClearSelections = 0x10,

        Undoable = 0x20,

        Modify = 0x40
    }
}
