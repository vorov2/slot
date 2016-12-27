using System;
using Slot.Core.ComponentModel;
using System.IO;
using Slot.Core.ViewModel;

namespace Slot.Core.Workspaces
{
    public interface IWorkspaceController : IComponent
    {
        bool OpenWorkspace(IView view, DirectoryInfo dir);
    }
}
