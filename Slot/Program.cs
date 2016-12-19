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
            //slotServer = new ApplicationServer();

            //var instance = slotServer.ConnectServer();
            
            //if (instance != null)
            //{
            //    instance.OpenView(args != null && args.Length > 0 ? args[0].Trim('"') : null);
            //    return;
            //}

            //slotServer.StartServer();
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
            App.Initialize();

            var frm = new MainForm();
            frm.Show();

            var ed = frm.Editor;

            //SettingsReader.Read(File.ReadAllText("samples\\settings.json"), ed);
            KeymapReader.Read(File.ReadAllText(LocalFile("samples\\keymap.json")), KeyboardAdapter.Instance);
            
            var theme = App.Catalog<IThemeComponent>().Default();
            theme.ChangeTheme((Identifier)"dark");

            var fl = LocalFile(@"..\..\test.htm");//@"c:\test\bigcode.cs";//
            var cmd = (Identifier)"file.openFile";
            App.Ext.Run(ed, cmd, fl);
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
        private const string CLIENT = "Slot.ApplicationClient";
        private const string INSTANCE = "Slot";
        private IpcChannel serverChannel;

        public void OpenView(string fileName = null)
        {
            var view = App.Catalog<IViewManager>().Default().CreateView();
            var buf = App.Catalog<IBufferManager>().Default();
            FileInfo fi;

            if (fileName != null && FileUtil.TryGetInfo(fileName, out fi))
                view.AttachBuffer(buf.CreateBuffer(fi, Encoding.UTF8));
            else
                view.AttachBuffer(buf.CreateBuffer());
        }

        public void StartServer()
        {
            serverChannel = new IpcChannel(SERVER);
            ChannelServices.RegisterChannel(serverChannel, false);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(ApplicationServer),
                INSTANCE, WellKnownObjectMode.SingleCall);
        }

        public ApplicationServer ConnectServer()
        {
            var obj = (ApplicationServer)Activator.GetObject(typeof(ApplicationServer),
                $"ipc://{SERVER}/{INSTANCE}");

            try
            {
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
