using Slot.Core;
using Slot.Core.CommandModel;
using Slot.Core.ComponentModel;
using Slot.Core.Output;
using Slot.Core.ViewModel;
using Slot.Core.Workspaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Slot.Main.Workspace
{
    [Export(typeof(ICommandDispatcher))]
    [ComponentData(Name)]
    public sealed class WorkspaceCommandDispatcher : CommandDispatcher
    {
        public const string Name = "workspace";

        [Command]
        public void OpenWorkspace(string dir)
        {
            var dirInfo = default(DirectoryInfo);

            if (!FileUtil.TryGetInfo(dir, out dirInfo))
            {
                App.Ext.Log($"Invalid directory path: {dir}", EntryType.Error);
                return;
            }

            var view = ViewManager.CreateView();

            if (App.Catalog<IWorkspaceController>().Default().OpenWorkspace(dirInfo))
            {
                if (view.Workspace.FullName.Equals(dirInfo.FullName, StringComparison.OrdinalIgnoreCase))
                {
                    var setDir = new DirectoryInfo(Path.Combine(dirInfo.FullName, ".slot"));

                    if (!setDir.Exists)
                    {
                        setDir.Create();
                        FileUtil.WriteFile(Path.Combine(setDir.FullName, "settings.json"), "//Put your settings here", Encoding.UTF8);
                    }
                }

                ViewManager.ActivateView(view);
            }
        }
    }
}
