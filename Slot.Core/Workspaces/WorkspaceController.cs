using System;
using System.ComponentModel.Composition;
using System.IO;
using Slot.Core.ComponentModel;
using Slot.Core.Settings;
using Slot.Core.ViewModel;

namespace Slot.Core.Workspaces
{
    [Export(typeof(IWorkspaceController))]
    [ComponentData(Name)]
    public sealed class WorkspaceController : IWorkspaceController
    {
        public const string Name = "workspaces.default";
        private const string FOLDER = ".slot";

        public bool OpenWorkspace(IView view, DirectoryInfo dir)
        {
            if (IsChildFolder(view.Workspace, dir))
                return false;

            var baseDir = dir;

            do
            {
                var ws = Path.Combine(dir.FullName, FOLDER);

                if (Directory.Exists(ws))
                    return DirectOpenWorkspace(view, dir);

                dir = dir.Parent;
            }
            while (dir != null);

            return DirectOpenWorkspace(view, baseDir);
        }

        private bool IsChildFolder(DirectoryInfo ws, DirectoryInfo dir)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, ".slot")))
                return false;

            if (ws == null)
                return false;

            do
            {
                if (string.Equals(ws.FullName, dir.FullName, StringComparison.OrdinalIgnoreCase))
                    return true;

                dir = dir.Parent;
            }
            while (dir != null);

            return false;
        }

        private bool DirectOpenWorkspace(IView view, DirectoryInfo dir)
        {
            if (!string.Equals(view.Workspace?.FullName, dir.FullName, StringComparison.OrdinalIgnoreCase))
            {
                FileUtil.EnsurePath(dir);
                Directory.SetCurrentDirectory(dir.FullName);
                view.Workspace = dir;
                view.Settings.ReloadSettings(SettingsScope.Workspace);
                return true;
            }

            return false;
        }
    }
}
