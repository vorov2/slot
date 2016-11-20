using CodeBox.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeBox.Test
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {

            
            ed.Settings.CurrentLineIndicator = false;
            ed.Settings.MatchBrackets = false;
            ed.Settings.ShowEol = false;
            ed.Settings.ShowLineLength = false;
            ed.Settings.ShowWhitespace = false;
            ed.Settings.WordWrap = false;
        }
    }
}
