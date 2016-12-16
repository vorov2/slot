using System;
using System.IO;

namespace Slot.Core.ViewModel
{
    public interface IView
    {
        void AttachBuffer(IBuffer buffer);

        void DetachBuffer();

        void Close();

        IBuffer Buffer { get; }

        DirectoryInfo Workspace { get; set; }

        string Mode { get; set; }
    }
}
