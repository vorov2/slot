using CodeBox.CommandBar;
using CodeBox.ComponentModel;
using CodeBox.Core;
using CodeBox.Core.CommandModel;
using CodeBox.Core.ComponentModel;
using CodeBox.Core.Keyboard;
using CodeBox.Core.ViewModel;
using CodeBox.Lexing;
using CodeBox.ObjectModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeBox.Test
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
            App.Initialize();

            var frm = new MainForm();
            frm.Show();

            var ed = frm.Editor;

            //SettingsReader.Read(File.ReadAllText("samples\\settings.json"), ed);
            KeymapReader.Read(File.ReadAllText(LocalFile("samples\\keymap.json")), KeyboardAdapter.Instance);
            CommandCatalog.Instance.RegisterCommands(CommandReader.Read(File.ReadAllText(LocalFile("samples\\commands.json"))));

            var theme = App.Catalog<IThemeComponent>().First();
            theme.ChangeTheme("dark");

            var fl = LocalFile(@"..\..\test.htm");//@"c:\test\bigcode.cs";//
            var cmd = (Identifier)"file.openfile";
            App.Ext.RunCommand(ed, cmd, fl);
            Application.Run();
        }
        private static string LocalFile(string fileName)
        {
            return Path.Combine(new FileInfo(typeof(MainForm).Assembly.Location).DirectoryName, fileName);
        }
    }
}
