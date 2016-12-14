using CodeBox.Core.ComponentModel;
using CodeBox.Core.ViewModel;
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

        [Import]
        private IViewManager viewManager = null;

        public void OpenWorkspace(DirectoryInfo dir)
        {
            var baseDir = dir;

            do
            {
                var ws = Path.Combine(dir.FullName, FOLDER);

                if (Directory.Exists(ws))
                {
                    DirectOpenWorkspace(dir);
                    return;
                }

                dir = dir.Parent;
            }
            while (dir != null);

            DirectOpenWorkspace(baseDir);
        }

        private void DirectOpenWorkspace(DirectoryInfo dir)
        {
            var view = viewManager.GetActiveView();
            view.Workspace = dir;
            Directory.SetCurrentDirectory(view.Workspace.FullName);
            OnWorkspaceChanged();
        }

        public event EventHandler WorkspaceChanged;
        private void OnWorkspaceChanged() => WorkspaceChanged?.Invoke(this, EventArgs.Empty);
    }
}
