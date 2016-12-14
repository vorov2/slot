using System;
using CodeBox.Core.ComponentModel;
using System.IO;

namespace CodeBox.Core.Workspaces
{
    public interface IWorkspaceController : IComponent
    {
        void OpenWorkspace(DirectoryInfo dir);

        event EventHandler WorkspaceChanged;
    }
}
