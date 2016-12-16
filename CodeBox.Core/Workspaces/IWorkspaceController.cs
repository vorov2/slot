using System;
using Slot.Core.ComponentModel;
using System.IO;

namespace Slot.Core.Workspaces
{
    public interface IWorkspaceController : IComponent
    {
        void OpenWorkspace(DirectoryInfo dir);

        event EventHandler WorkspaceChanged;
    }
}
