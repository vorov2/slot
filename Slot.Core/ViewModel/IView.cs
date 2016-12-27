using System;
using System.IO;
using Slot.Core.Settings;
using Slot.Core.Themes;

namespace Slot.Core.ViewModel
{
    public interface IView
    {
        void AttachBuffer(IBuffer buffer);

        void DetachBuffer();

        object CommandBar { get; }

        IEditor Editor { get; }

        IBuffer Buffer { get; }

        DirectoryInfo Workspace { get; set; }

        Identifier Mode { get; set; }

        ISettings Settings { get; }
    }
}
