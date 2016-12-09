using System;
using CodeBox.Core.ComponentModel;
using System.IO;

namespace CodeBox.Core.Workspaces
{
    public interface IWorkspaceController : IComponent
    {
        void CreateWorkspace(DirectoryInfo dir);

        bool OpenWorkspace(DirectoryInfo dir);

        DirectoryInfo CurrentWorkspace { get; }

        event EventHandler WorkspaceOpened;
    }
}
