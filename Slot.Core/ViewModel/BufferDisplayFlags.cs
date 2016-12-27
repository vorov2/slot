using System;

namespace Slot.Core.ViewModel
{
    [Flags]
    public enum BufferDisplayFlags
    {
        None = 0xFF,

        HideCommandBar = 0x01,

        HideStatusBar = 0x02
    }
}
