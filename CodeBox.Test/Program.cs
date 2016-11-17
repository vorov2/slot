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

            var ka = new KeyboardAdapter();
            ka.RegisterInput("a", "Ctrl+E,A");
            ka.RegisterInput("b", "Ctrl+E,B");
            ka.RegisterInput("c", "Ctrl+E,Ctrl+C,A");


            Application.Run(new MainForm());
        }
    }
}
