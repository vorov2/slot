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

namespace Slot
{
    static class Program
    {
        private static Process applicationProcess;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            //applicationProcess = Process.GetCurrentProcess();
            //var p = ProcessController.FindProcess();

            //if (p != null)
            //{
            //    p.StandardInput.WriteLine($"Hello from {applicationProcess.Id}");
                
            //    return;
            //}

            applicationProcess.StartInfo.RedirectStandardInput = true;
            applicationProcess.StartInfo.RedirectStandardOutput = true;
            applicationProcess.EnableRaisingEvents = true;
            applicationProcess.Refresh();
            applicationProcess.OutputDataReceived += ApplicationProcess_OutputDataReceived;

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

        private static void ApplicationProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            MessageBox.Show(e.Data);
        }

        private static string LocalFile(string fileName)
        {
            return Path.Combine(new FileInfo(typeof(MainForm).Assembly.Location).DirectoryName, fileName);
        }
    }

    public static class ProcessController
    {
        public static void OpenView(string fileName = null)
        {
            var view = App.Catalog<IViewManager>().Default().CreateView();
            var buf = App.Catalog<IBufferManager>().Default();
            FileInfo fi;

            if (fileName != null && FileUtil.TryGetInfo(fileName, out fi))
                view.AttachBuffer(buf.CreateBuffer(fi, Encoding.UTF8));
            else
                view.AttachBuffer(buf.CreateBuffer());
        }

        public static Process FindProcess()
        {
            var cur = Process.GetCurrentProcess();
            return Process.GetProcessesByName("Slot"/*cur.ProcessName*/).FirstOrDefault(p => p.Id != cur.Id);
        }
    }
}
