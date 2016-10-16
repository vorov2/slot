using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CodeBox.Margins;

namespace CodeBox.Test
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            editor1.LeftMargins.Add(new LineNumberMargin { MarkCurrentLine = true });
            editor1.LeftMargins.Add(new GutterMargin());
            editor1.Text = File.ReadAllText("C:\\test\\code.cs");
        }
        

        private void Form1_Activated(object sender, EventArgs e)
        {
            editor1.Focus();
        }
        

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            editor1.Undo();
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            editor1.Redo();
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            editor1.Cut();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            editor1.Copy();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            editor1.Paste();
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            editor1.SelectAll();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            Focus();
        }
    }
}
