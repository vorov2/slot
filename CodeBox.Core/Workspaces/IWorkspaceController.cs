using System;
using CodeBox.Core.ComponentModel;
using System.IO;

namespace CodeBox.Core.Workspaces
{
    public interface IWorkspaceController : IComponent
    {
        void CreateWorkspace(DirectoryInfo dir);

        bool OpenWorkspace(DirectoryInfo dir);

        void CloseWorkspace();

        DirectoryInfo CurrentWorkspace { get; }

        event EventHandler WorkspaceChanged;
    }
}
