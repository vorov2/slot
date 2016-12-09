using CodeBox.Core.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Core.Workspaces
{
    [Export(typeof(IWorkspaceController))]
    [ComponentData(Name)]
    public sealed class WorkspaceController : IWorkspaceController
    {
        public const string Name = "workspaces.default";
        private const string FOLDER = ".codebox";

        public void CreateWorkspace(DirectoryInfo dir)
        {
            var ws = Path.Combine(dir.FullName, FOLDER);

            if (!Directory.Exists(ws))
            {
                Directory.CreateDirectory(ws);
            }

            OpenWorkspace(dir);
        }

        public bool OpenWorkspace(DirectoryInfo dir)
        {
            if (dir == null)
                return false;

            var ws = Path.Combine(dir.FullName, FOLDER);

            if (Directory.Exists(ws))
            {
                CurrentWorkspace = dir;
                Directory.SetCurrentDirectory(dir.FullName);
                OnWorkspaceOpened();
                return true;
            }

            return OpenWorkspace(dir.Parent);
        }

        public DirectoryInfo CurrentWorkspace { get; private set; }

        public event EventHandler WorkspaceOpened;
        private void OnWorkspaceOpened() => WorkspaceOpened?.Invoke(this, EventArgs.Empty);
    }
}
