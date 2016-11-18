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

            ed.Styles.Selection.BackColor = ColorTranslator.FromHtml("#264F78");
            ed.Styles.Default.ForeColor = ColorTranslator.FromHtml("#DCDCDC");
            ed.Styles.Default.BackColor = ColorTranslator.FromHtml("#1E1E1E");
            ed.Settings.CurrentLineIndicator = false;
            ed.Settings.MatchBrackets = false;
            ed.Settings.ShowEol = false;
            ed.Settings.ShowLineLength = false;
            ed.Settings.ShowWhitespace = false;
            ed.Settings.WordWrap = false;
        }
    }
}
