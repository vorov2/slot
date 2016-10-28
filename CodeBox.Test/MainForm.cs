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
using CodeBox.Commands;

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
            BindCommands();
            ed.Styles.StyleNeeded += editor1_StyleNeeded;
            ed.LeftMargins.Add(new LineNumberMargin(ed) { MarkCurrentLine = true });
            ed.LeftMargins.Add(new GutterMargin(ed));
            ed.RightMargins.Add(new ScrollBarMargin(ed));
            ed.BottomMargins.Add(new ScrollBarMargin(ed));
            ed.TopMargins.Add(new TopMargin(ed));
            ed.Styles.Default.ForeColor = ColorTranslator.FromHtml("#DCDCDC");
            ed.Styles.Default.BackColor = ColorTranslator.FromHtml("#1E1E1E");
            ed.Styles.LineNumber.ForeColor = ColorTranslator.FromHtml("#505050");
            ed.Styles.LineNumber.BackColor = ColorTranslator.FromHtml("#1E1E1E");
            ed.Styles.CurrentLineNumber.ForeColor = ColorTranslator.FromHtml("#848484");
            ed.Styles.CurrentLineNumber.BackColor = ColorTranslator.FromHtml("#262626");
            ed.Styles.SpecialSymbol.ForeColor = ColorTranslator.FromHtml("#505050");
            ed.Styles.Selection.Color = ColorTranslator.FromHtml("#264F78");
            ed.Styles.Register(110,
                new TextStyle {
                    ForeColor = ColorTranslator.FromHtml("#8CDCDB")
                });
            ed.Styles.Register(111,
                new TextStyle {
                    ForeColor = ColorTranslator.FromHtml("#D69D85")
                });
            ed.Styles.Register(112,
                new TextStyle {
                    ForeColor = ColorTranslator.FromHtml("#579032")
                });
            ed.Text = File.ReadAllText(
                Path.Combine(new FileInfo(typeof(MainForm).Assembly.Location).DirectoryName,"test.json"));
        }

        private void BindCommands()
        {
            ed.CommandManager.Bind<RedoCommand>(Keys.Control | Keys.Y);
            ed.CommandManager.Bind<UndoCommand>(Keys.Control | Keys.Z);
            ed.CommandManager.Bind<SelectAllCommand>(Keys.Control | Keys.A);
            ed.CommandManager.Bind<PasteCommand>(Keys.Control | Keys.V);
            ed.CommandManager.Bind<CutCommand>(Keys.Control | Keys.X);
            ed.CommandManager.Bind<CopyCommand>(Keys.Control | Keys.C);
            ed.CommandManager.Bind<SetCaretCommand>(MouseEvents.Click, Keys.None);
            ed.CommandManager.Bind<AddCaretCommand>(MouseEvents.Click, Keys.Control);
            ed.CommandManager.Bind<NormalSelectCommand>(MouseEvents.Move | MouseEvents.Click, Keys.Control);
            ed.CommandManager.Bind<NormalSelectCommand>(MouseEvents.Move | MouseEvents.Click, Keys.None);
            ed.CommandManager.Bind<BlockSelectCommand>(MouseEvents.Move | MouseEvents.Click, Keys.Alt);
            ed.CommandManager.Bind<SelectWordCommand>(MouseEvents.DoubleClick, Keys.None);
            ed.CommandManager.Bind<ShiftTabCommand>(Keys.Shift | Keys.Tab);
            ed.CommandManager.Bind<TabCommand>(Keys.Tab);
            ed.CommandManager.Bind<ClearSelectionCommand>(Keys.Escape);
            ed.CommandManager.Bind<LeftCommand>(Keys.Left);
            ed.CommandManager.Bind<RightCommand>(Keys.Right);
            ed.CommandManager.Bind<UpCommand>(Keys.Up);
            ed.CommandManager.Bind<DownCommand>(Keys.Down);
            ed.CommandManager.Bind<HomeCommand>(Keys.Home);
            ed.CommandManager.Bind<EndCommand>(Keys.End);
            ed.CommandManager.Bind<InsertNewLineCommand>(Keys.Enter);
            ed.CommandManager.Bind<DeleteBackCommand>(Keys.Back);
            ed.CommandManager.Bind<DeleteCommand>(Keys.Delete);
            ed.CommandManager.Bind<PageDownCommand>(Keys.PageDown);
            ed.CommandManager.Bind<PageUpCommand>(Keys.PageUp);
            ed.CommandManager.Bind<ExtendLeftCommand>(Keys.Shift | Keys.Left);
            ed.CommandManager.Bind<ExtendRightCommand>(Keys.Shift | Keys.Right);
            ed.CommandManager.Bind<ExtendUpCommand>(Keys.Shift | Keys.Up);
            ed.CommandManager.Bind<ExtendDownCommand>(Keys.Shift | Keys.Down);
            ed.CommandManager.Bind<ExtendEndCommand>(Keys.Shift | Keys.End);
            ed.CommandManager.Bind<ExtendHomeCommand>(Keys.Shift | Keys.Home);
            ed.CommandManager.Bind<WordLeftCommand>(Keys.Control | Keys.Left);
            ed.CommandManager.Bind<WordRightCommand>(Keys.Control | Keys.Right);
            ed.CommandManager.Bind<ExtendWordRightCommandCommand>(Keys.Control | Keys.Shift | Keys.Right);
            ed.CommandManager.Bind<ExtendWordLeftCommandCommand>(Keys.Control | Keys.Shift | Keys.Left);
            ed.CommandManager.Bind<ExtendPageDownCommand>(Keys.Control | Keys.PageDown);
            ed.CommandManager.Bind<ExtendPageUpCommand>(Keys.Control | Keys.PageUp);
            ed.CommandManager.Bind<DocumentHomeCommand>(Keys.Control | Keys.Home);
            ed.CommandManager.Bind<DocumentEndCommand>(Keys.Control | Keys.End);
            ed.CommandManager.Bind<ExtendDocumentHomeCommand>(Keys.Control | Keys.Shift | Keys.Home);
            ed.CommandManager.Bind<ExtendDocumentEndCommand>(Keys.Control | Keys.Shift | Keys.End);
        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            ed.Focus();
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
                    ed.Styles.StyleRange(st ? 110 : 111, line, i, ei);
                    i = ei + 1;
                }
                else if (c == '/' && i < str.Length - 1 && str[i + 1] == '*')
                {
                    bool end;
                    var ei = ParseComment(i + 1, str, out end);
                    //ed.Styles.StyleRange(12,
                    //    new Range(new Pos(line, i), new Pos(line, ei)));
                    ed.Styles.StyleRange(112, line, i, ei);
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
            ed.Styles.StyleRange(112, line, 0, i);

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
