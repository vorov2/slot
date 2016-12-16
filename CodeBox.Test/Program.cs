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
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
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
            var cmd = (Identifier)"file.openfile";
            App.Ext.Run(ed, cmd, fl);
            Application.Run();
        }
        private static string LocalFile(string fileName)
        {
            return Path.Combine(new FileInfo(typeof(MainForm).Assembly.Location).DirectoryName, fileName);
        }
    }
}
