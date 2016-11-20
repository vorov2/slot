using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

            var cp = new CommandParser();
            var stmt = cp.Parse("fo file.txt;delline 12 ; fsa 'c:\\test\\long file name.txt'").ToArray();

            Application.Run(new MainForm());
        }
    }
}
