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
using CodeBox.Folding;

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
            ed.Folding.FoldingNeeded += Folding_FoldingNeeded;
            ed.LeftMargins.Add(new LineNumberMargin(ed) { MarkCurrentLine = true });
            ed.LeftMargins.Add(new FoldingMargin(ed));
           // ed.LeftMargins.Add(new GutterMargin(ed));
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
            ed.Styles.FoldingMarker.ForeColor = ColorTranslator.FromHtml("#505050");
            ed.Styles.FoldingMarker.BackColor = ColorTranslator.FromHtml("#1E1E1E");
            ed.Styles.ActiveFoldingMarker.ForeColor = ColorTranslator.FromHtml("#C0AAF7");
            ed.Styles.ActiveFoldingMarker.BackColor = ColorTranslator.FromHtml("#1E1E1E");
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
            ed.Text = File.ReadAllText(//@"C:\Test\bigcode.cs");
                Path.Combine(new FileInfo(typeof(MainForm).Assembly.Location).DirectoryName, "test.json"));
        }

        private void Folding_FoldingNeeded(object sender, Folding.FoldingNeededEventArgs e)
        {
            Console.WriteLine("Folding...");
            var li = e.Range.Start.Line;

            while (li > -1 && !ed.Document.Lines[li].Folding.Has(FoldingStates.Header))
                li--;

            li = li < 0 ? 0 : li;
            var prevIndent = 0;

            if (li > 0)
            {
                var ln = ed.Document.Lines[li - 1];

                for (var i = 0; ; i++)
                {
                    var c = ln.CharAt(i);

                    if (c == ' ' || c == '\t')
                        prevIndent++;
                    else
                        break;
                }
            }

            var initIndent = prevIndent;

            for (var i = li; i < ed.Document.Lines.Count; i++)
            {
                var line = ed.Document.Lines[i];
                var txt = line.Text;
                var indent = 0;

                foreach (var c in txt)
                {
                    if (c == ' ')
                        indent++;
                    else if (c == '\t')
                        indent += ed.Settings.TabSize;
                    else
                        break;
                }

                indent /= ed.Settings.TabSize;

                if (indent > prevIndent && i > 0)
                {
                    var ln = ed.Document.Lines[i - 1];
                    ed.Document.Lines[i - 1].Folding = FoldingStates.Header
                        | (ln.Folding.Has(FoldingStates.Invisible) ? FoldingStates.Invisible : FoldingStates.None);
                    line.Folding &= ~FoldingStates.Header;
                    line.Folding &= ~FoldingStates.Footer;
                }
                else if (indent == prevIndent)
                {
                    line.Folding &= ~FoldingStates.Header;
                    line.Folding &= ~FoldingStates.Footer;
                    //line.Visible = VisibleState.Invisible;
                }
                else if (prevIndent > indent)
                {
                    line.Folding = FoldingStates.Footer
                        | (line.Folding.Has(FoldingStates.Invisible) ? FoldingStates.Invisible : FoldingStates.None);
                }
                //else
                //    line.Visible |= VisibleStates.Visible;

                if (i >= e.Range.End.Line && indent == initIndent)
                    break;

                prevIndent = indent;
            }
        }

        private void BindCommands()
        {
            ed.Commands.Bind<DeleteWordBackCommand>(Keys.Control | Keys.Back);
            ed.Commands.Bind<DeleteWordCommand>(Keys.Control | Keys.Delete);
            ed.Commands.Bind<ScrollLineUpCommand>(Keys.Control | Keys.Up);
            ed.Commands.Bind<ScrollLineDownCommand>(Keys.Control | Keys.Down);
            ed.Commands.Bind<RedoCommand>(Keys.Control | Keys.Y);
            ed.Commands.Bind<UndoCommand>(Keys.Control | Keys.Z);
            ed.Commands.Bind<SelectAllCommand>(Keys.Control | Keys.A);
            ed.Commands.Bind<PasteCommand>(Keys.Control | Keys.V);
            ed.Commands.Bind<CutCommand>(Keys.Control | Keys.X);
            ed.Commands.Bind<CopyCommand>(Keys.Control | Keys.C);
            ed.Commands.Bind<SetCaretCommand>(MouseEvents.Click, Keys.Alt);
            ed.Commands.Bind<SetCaretCommand>(MouseEvents.Click, Keys.None);
            ed.Commands.Bind<AddCaretCommand>(MouseEvents.Click, Keys.Control);
            ed.Commands.Bind<NormalSelectCommand>(MouseEvents.Move | MouseEvents.Click, Keys.Control);
            ed.Commands.Bind<NormalSelectCommand>(MouseEvents.Move | MouseEvents.Click, Keys.None);
            ed.Commands.Bind<BlockSelectCommand>(MouseEvents.Move | MouseEvents.Click, Keys.Alt);
            ed.Commands.Bind<SelectWordCommand>(MouseEvents.DoubleClick, Keys.None);
            ed.Commands.Bind<ShiftTabCommand>(Keys.Shift | Keys.Tab);
            ed.Commands.Bind<TabCommand>(Keys.Tab);
            ed.Commands.Bind<ClearSelectionCommand>(Keys.Escape);
            ed.Commands.Bind<LeftCommand>(Keys.Left);
            ed.Commands.Bind<RightCommand>(Keys.Right);
            ed.Commands.Bind<UpCommand>(Keys.Up);
            ed.Commands.Bind<DownCommand>(Keys.Down);
            ed.Commands.Bind<HomeCommand>(Keys.Home);
            ed.Commands.Bind<EndCommand>(Keys.End);
            ed.Commands.Bind<InsertNewLineCommand>(Keys.Enter);
            ed.Commands.Bind<DeleteBackCommand>(Keys.Back);
            ed.Commands.Bind<DeleteCommand>(Keys.Delete);
            ed.Commands.Bind<PageDownCommand>(Keys.PageDown);
            ed.Commands.Bind<PageUpCommand>(Keys.PageUp);
            ed.Commands.Bind<ExtendLeftCommand>(Keys.Shift | Keys.Left);
            ed.Commands.Bind<ExtendRightCommand>(Keys.Shift | Keys.Right);
            ed.Commands.Bind<ExtendUpCommand>(Keys.Shift | Keys.Up);
            ed.Commands.Bind<ExtendDownCommand>(Keys.Shift | Keys.Down);
            ed.Commands.Bind<ExtendEndCommand>(Keys.Shift | Keys.End);
            ed.Commands.Bind<ExtendHomeCommand>(Keys.Shift | Keys.Home);
            ed.Commands.Bind<WordLeftCommand>(Keys.Control | Keys.Left);
            ed.Commands.Bind<WordRightCommand>(Keys.Control | Keys.Right);
            ed.Commands.Bind<ExtendWordRightCommandCommand>(Keys.Control | Keys.Shift | Keys.Right);
            ed.Commands.Bind<ExtendWordLeftCommandCommand>(Keys.Control | Keys.Shift | Keys.Left);
            ed.Commands.Bind<ExtendPageDownCommand>(Keys.Control | Keys.PageDown);
            ed.Commands.Bind<ExtendPageUpCommand>(Keys.Control | Keys.PageUp);
            ed.Commands.Bind<DocumentHomeCommand>(Keys.Control | Keys.Home);
            ed.Commands.Bind<DocumentEndCommand>(Keys.Control | Keys.End);
            ed.Commands.Bind<ExtendDocumentHomeCommand>(Keys.Control | Keys.Shift | Keys.Home);
            ed.Commands.Bind<ExtendDocumentEndCommand>(Keys.Control | Keys.Shift | Keys.End);
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

            li = li < 0 ? 0 : li;

            for (var i = li; i < e.Range.End.Line + 1; i++)
            {
                ed.Styles.ClearStyles(i);
                var line = ed.Document.Lines[i];
                var txt = line.Text;

                if (state == 2)
                    state = ed.Document.Lines[i].State = ParseComment(i, txt);
                else
                    state = ed.Document.Lines[i].State = Parse(i, txt, 0);
            }
        }

        private byte Parse(int line, string str, int pos)
        {
            byte state = 1;
            var nonempty = pos != 0;

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
