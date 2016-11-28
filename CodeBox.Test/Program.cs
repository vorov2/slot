using CodeBox.CommandLine;
using CodeBox.ComponentModel;
using CodeBox.Core;
using CodeBox.Core.ComponentModel;
using CodeBox.Core.Keyboard;
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
            var frm = new MainForm();
            var ed = frm.Editor;

            //SettingsReader.Read(File.ReadAllText("samples\\settings.json"), ed);
            KeymapReader.Read(File.ReadAllText(LocalFile("samples\\keymap.json")), KeyboardAdapter.Instance);

            var theme = ComponentCatalog.Instance.GetComponent((Identifier)"theme.default") as IThemeComponent;
            theme.ChangeTheme("dark");

            var fl = LocalFile("test.htm");//@"c:\test\bigcode.cs";//
            ed.AttachBuffer(new DocumentBuffer(Document.FromString(File.ReadAllText(fl)), new FileInfo(fl), Encoding.UTF8));


            Application.Run(frm);
        }
        private static string LocalFile(string fileName)
        {
            return Path.Combine(new FileInfo(typeof(MainForm).Assembly.Location).DirectoryName, fileName);
        }
    }
}
