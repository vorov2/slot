using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Slot.ComponentModel;
using Slot.Core;
using Slot.Core.CommandModel;
using Slot.Core.ComponentModel;
using Slot.Core.Keyboard;
using Slot.Core.Messages;
using Slot.Core.Modes;
using Slot.Core.Output;
using Slot.Core.Packages;
using Slot.Core.Settings;
using Slot.Core.State;
using Slot.Core.Themes;
using Slot.Core.ViewModel;
using Slot.Core.Workspaces;
using Slot.Editor.ComponentModel;
using Slot.Editor.Lexing;
using Slot.Main;
using Slot.Main.View;

namespace Slot
{
    static class Program
    {
        private static ApplicationServer slotServer;

        [STAThread]
        static void Main(string[] args)
        {
            var fileName = args != null && args.Length > 0 ? args[0].Trim('"') : null;
            slotServer = new ApplicationServer();
            var instance = slotServer.ConnectServer();

            if (instance != null)
            {
                instance.OpenView(fileName);
                return;
            }

            slotServer.StartServer();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            App.RegisterCatalog<IComponent>();
            App.RegisterCatalog<ICommandDispatcher>();
            App.RegisterCatalog<ICommandBar>();
            App.RegisterCatalog<IArgumentValueProvider>();
            App.RegisterCatalog<ITheme>();
            App.RegisterCatalog<IDentComponent>();
            App.RegisterCatalog<IFoldingComponent>();
            App.RegisterCatalog<IStylerComponent>();
            App.RegisterCatalog<IGrammarComponent>();
            App.RegisterCatalog<IBufferManager>();
            App.RegisterCatalog<IViewManager>();
            App.RegisterCatalog<ILogComponent>();
            App.RegisterCatalog<ISettingsManager>();
            App.RegisterCatalog<IWorkspaceController>();
            App.RegisterCatalog<ICommandProvider>();
            App.RegisterCatalog<IModeManager>();
            App.RegisterCatalog<IPackageManager>();
            App.RegisterCatalog<IKeyboardAdapter>();
            App.RegisterCatalog<IStateManager>();
            App.RegisterCatalog<IMessageBox>();
            App.Initialize();

            var view = App.Component<IViewManager>().CreateView();

            FileInfo fi;

            if (fileName != null && FileUtil.TryGetInfo(fileName, out fi) && fi.Exists)
                App.Ext.Run(Cmd.OpenFile, fi.FullName);
            else
            {
                var buf = App.Component<IBufferManager>().EnumerateBuffers().FirstOrDefault();

                if (buf != null)
                    App.Ext.Run(Cmd.OpenFile, buf.File.FullName);
                else
                    App.Ext.Run(Cmd.NewFile);
            }

            Application.EnableVisualStyles();
            Application.ApplicationExit += (o, e) => slotServer.StopServer();
            Application.Run();
        }
    }
}
