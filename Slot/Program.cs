using Slot.ComponentModel;
using Slot.Core;
using Slot.Core.CommandModel;
using Slot.Core.ComponentModel;
using Slot.Core.Keyboard;
using Slot.Core.Output;
using Slot.Core.Settings;
using Slot.Core.Themes;
using Slot.Core.ViewModel;
using Slot.Core.Workspaces;
using Slot.Editor.Lexing;
using Slot.Main;
using Slot.Editor.ObjectModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Slot.Editor.ComponentModel;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Security.Permissions;
using Slot.Core.Modes;
using Slot.Core.Packages;

namespace Slot
{
    static class Program
    {
        private static ApplicationServer slotServer;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
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
            App.RegisterCatalog<IThemeComponent>();
            App.RegisterCatalog<IDentComponent>();
            App.RegisterCatalog<IFoldingComponent>();
            App.RegisterCatalog<IStylerComponent>();
            App.RegisterCatalog<IGrammarComponent>();
            App.RegisterCatalog<IBufferManager>();
            App.RegisterCatalog<IViewManager>();
            App.RegisterCatalog<ILogComponent>();
            App.RegisterCatalog<ISettingsProvider>();
            App.RegisterCatalog<IWorkspaceController>();
            App.RegisterCatalog<ICommandProvider>();
            App.RegisterCatalog<IModeManager>();
            App.RegisterCatalog<IPackageManager>();
            App.RegisterCatalog<IKeyboardAdapter>();
            App.Initialize();

            var frm = new MainForm();
            frm.Show();

            var ed = frm.Editor;

            //SettingsReader.Read(File.ReadAllText("samples\\settings.json"), ed);
            //KeymapReader.Read(File.ReadAllText(LocalFile("samples\\keymap.json")), KeyboardAdapter.Instance);

            var theme = App.Catalog<IThemeComponent>().Default();
            theme.ChangeTheme((Identifier)"dark");

            FileInfo fi;

            if (fileName != null && FileUtil.TryGetInfo(fileName, out fi) && fi.Exists)
                App.Ext.Run(ed, Slot.Main.File.Cmd.OpenFile, fi.FullName);

            App.Catalog<IKeyboardAdapter>().Default().ChangeKeymap((Identifier)"default");
            Application.ApplicationExit += (o, e) => slotServer.StopServer();
            Application.Run();
        }
        private static string LocalFile(string fileName)
        {
            return Path.Combine(new FileInfo(typeof(MainForm).Assembly.Location).DirectoryName, fileName);
        }
    }

    [Serializable]
    public sealed class ApplicationServer : MarshalByRefObject
    {
        private const string SERVER = "Slot.ApplicationServer";
        private const string INSTANCE = "Slot";
        private const string LOCK = "Slot-19645371-4E1B-4517-9CC9-1D5F5AA618B7";
        private IpcChannel serverChannel;
        private FileStream fileStream;

        private string FileName => Path.Combine(Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData), LOCK);

        public void OpenView(string fileName = null)
        {
            Application.OpenForms[0].Invoke((MethodInvoker)(() =>
            {
                var view = App.Catalog<IViewManager>().Default().CreateView();
                var buf = App.Catalog<IBufferManager>().Default();
                FileInfo fi;

                if (fileName != null && FileUtil.TryGetInfo(fileName, out fi))
                    view.AttachBuffer(buf.CreateBuffer(fi, Encoding.UTF8));
                else
                    view.AttachBuffer(buf.CreateBuffer());

                App.Catalog<IViewManager>().Default().ActivateView(view);
            }));
        }

        public void StopServer()
        {
            fileStream.Unlock(0, fileStream.Length);
            fileStream.Dispose();
            ChannelServices.UnregisterChannel(serverChannel);
            Process.GetCurrentProcess().Kill();
        }

        public void StartServer()
        {
            fileStream = ObtainLock();
            fileStream.Lock(0, fileStream.Length);
            serverChannel = new IpcChannel(SERVER);
            ChannelServices.RegisterChannel(serverChannel, false);
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(ApplicationServer), INSTANCE,
                WellKnownObjectMode.Singleton);
        }

        private FileStream ObtainLock()
        {
            try
            {
                return File.Open(FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            }
            catch //TODO
            {
                return null;
            }
        }

        public ApplicationServer ConnectServer()
        {
            var fs = ObtainLock();

            if (fs != null)
            {
                fs.Close();
                return null;
            }

            try
            {
                var obj = Activator.GetObject(typeof(ApplicationServer),
                    $"ipc://{SERVER}/{INSTANCE}") as ApplicationServer;

                var _ = obj.Ping(); //TODO: ugly
                return obj;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public Guid Ping()
        {
            return Id;
        }

        public Guid Id { get; } = Guid.NewGuid();
    }
}
