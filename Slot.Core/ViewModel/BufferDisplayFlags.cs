using System;

namespace Slot.Core.ViewModel
{
    [Flags]
    public enum BufferDisplayFlags
    {
        None = 0xFF,

        HideHeader = 0x01,

        HideStatusBar = 0x02,

        HideWorkspace = 0x04,

        ReadOnly = 0x08
    }
}
