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
using CodeBox.ObjectModel;
using CodeBox.Styling;

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
            ed.LeftMargins.Add(new LineNumberMargin(ed) { MarkCurrentLine = true });
            ed.LeftMargins.Add(new GutterMargin(ed));
            ed.RightMargins.Add(new ScrollBarMargin(ed));
            ed.BottomMargins.Add(new ScrollBarMargin(ed));
            ed.TopMargins.Add(new TopMargin(ed));

            ed.Styles.Register(StandardStyle.Default,
                new StyleInfo(
                    ColorTranslator.FromHtml("#DCDCDC"),
                    ColorTranslator.FromHtml("#1E1E1E"),
                    FontStyle.Regular
                ));
            ed.Styles.Register(StandardStyle.LineNumber,
                new StyleInfo(
                    ColorTranslator.FromHtml("#505050")
                ));
            ed.Styles.Register(StandardStyle.CurrentLineNumber,
                new StyleInfo(
                    ColorTranslator.FromHtml("#848484"),
                    ColorTranslator.FromHtml("#262626")
                ));
            ed.Styles.Register(StandardStyle.SpecialSymbol,
                new StyleInfo(
                    ColorTranslator.FromHtml("#505050")
                ));
            ed.Styles.Register(10,
                new StyleInfo(
                    ColorTranslator.FromHtml("#8CDCDB")
                ));
            ed.Styles.Register(11,
                new StyleInfo(
                    ColorTranslator.FromHtml("#D69D85")
                ));
            ed.Styles.Register(12,
                new StyleInfo(
                    ColorTranslator.FromHtml("#579032")
                ));

            ed.Text = File.ReadAllText(
                Path.Combine(new FileInfo(typeof(MainForm).Assembly.Location).DirectoryName,"test.json"));
        }
        

        private void Form1_Activated(object sender, EventArgs e)
        {
            ed.Focus();
        }
        

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ed.Undo();
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ed.Redo();
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ed.Cut();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ed.Copy();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ed.Paste();
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ed.SelectAll();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            Focus();
        }
        
        private void editor1_StyleNeeded(object sender, StyleNeededEventArgs e)
        {
            //Console.WriteLine("StyleNeeded:"+count++);
            //var txt = editor1.GetTextRange(e.Range);
            var state = 0;// e.Range.Start.Line > 0 ? editor1.Document.Lines[e.Range.Start.Line - 1].State : 0;
            var li = e.Range.Start.Line;

            while (li > -1 && (state = ed.Document.Lines[li].State) == 0)
                li--;


            for (var i = e.Range.Start.Line; i < e.Range.End.Line + 1; i++)
            {
                ed.Styles.ClearStyles(i);
                var txt = ed.Document.Lines[i].Text;

                if (state == 2)
                    state = ed.Document.Lines[i].State = ParseComment(i, txt);
                else
                    state = ed.Document.Lines[i].State = Parse(i, txt);

            }


        }


        private byte Parse(int line, string str, int pos = 0)
        {
            byte state = 1;

            for (var i = pos; i < str.Length; i++)
            {
                var c = str[i];

                if (c == '"')
                {
                    var ei = ParseString(i + 1, str);
                    var st = FindSemi(ei + 1, str);
                    //ed.Styles.StyleRange(st ? (byte)10 : (byte)11, 
                    //    new Range(new Pos(line, i), new Pos(line, ei)));
                    ed.Styles.StyleRange(st ? 10 : 11, line, i, ei);
                    i = ei + 1;
                }
                else if (c == '/' && i < str.Length - 1 && str[i + 1] == '*')
                {
                    bool end;
                    var ei = ParseComment(i + 1, str, out end);
                    //ed.Styles.StyleRange(12,
                    //    new Range(new Pos(line, i), new Pos(line, ei)));
                    ed.Styles.StyleRange(12, line, i, ei);
                    i = ei + 1;
                    if (!end)
                        state = 2;
                }
            }

            return state;
        }

        private byte ParseComment(int line, string str)
        {
            bool end;
            var i = ParseComment(0, str, out end);
            ed.Styles.StyleRange(12, line, 0, i);

            if (!end)
                return 2;
            else
                return Parse(line, str, i + 1);
        }

        private int ParseComment(int i, string str, out bool end)
        {
            end = false;

            for (; i < str.Length; i++)
            {
                var c = str[i];
                if (c == '*' && i < str.Length - 1 && str[i + 1] == '/')
                {
                    end = true;
                    return i + 1;
                }
            }

            return str.Length - 1;
        }

        private bool FindSemi(int i, string str)
        {
            for (; i < str.Length; i++)
            {
                var c = str[i];
                if (c == ':')
                    return true;
                else if (c != ' ' && c == '\t')
                    return false;
            }

            return false;
        }

        private int ParseString(int i, string str)
        {
            for (; i < str.Length; i++)
            {
                if (str[i] == '"' && (str[i - 1] != '\\' || (i > 1 && str[i - 2] == '\\')))
                    return i;
            }

            return str.Length - 1;
        }
    }
}
