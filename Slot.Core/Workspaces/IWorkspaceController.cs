using System;
using Slot.Core.ComponentModel;
using System.IO;

namespace Slot.Core.Workspaces
{
    public interface IWorkspaceController : IComponent
    {
        bool OpenWorkspace(DirectoryInfo dir);
    }
}
