using System;

namespace Slot.Editor.Margins
{
    [Flags]
    public enum MarginEffects
    {
        None,

        Redraw = 0x01,

        Invalidate = 0x02,

        Scroll = 0x04,

        CaptureMouse = 0x08
    }
}
