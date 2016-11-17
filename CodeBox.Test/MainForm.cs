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
using CodeBox.Lexing;
using CodeBox.Indentation;
using CodeBox.Affinity;

namespace CodeBox.Test
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            base.KeyPreview = true;
        }

        private Color HCol(string str)
        {
            return ColorTranslator.FromHtml(str);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            BindCommands();

            //lexer.GrammarProvider.RegisterGrammar(HtmlGrammar2());
            //lexer.GrammarProvider.RegisterGrammar(CsGrammar());
            //lexer.GrammarProvider.RegisterGrammar(CssGrammar());
            //lexer.GrammarKey = "html";
            ed.Styles.StylerKey = "styler.lexer";
            var lexer = (ConfigurableLexer)ed.Styles.Styler;
            var csharp = GrammarReader.Read(File.ReadAllText(LocalFile("grammars\\csharp.grammar.json")));
            var csharpExp = GrammarReader.Read(File.ReadAllText(LocalFile("grammars\\csharp-expression.grammar.json")));
            var html = GrammarReader.Read(File.ReadAllText(LocalFile("grammars\\html.grammar.json")));
            var css = GrammarReader.Read(File.ReadAllText(LocalFile("grammars\\css.grammar.json")));
            lexer.GrammarProvider.RegisterGrammar(csharp);
            lexer.GrammarProvider.RegisterGrammar(csharpExp);
            lexer.GrammarProvider.RegisterGrammar(html);
            lexer.GrammarProvider.RegisterGrammar(css);
            lexer.GrammarKey = "html";


            ed.LeftMargins.Add(new LineNumberMargin(ed) { MarkCurrentLine = true });
            ed.LeftMargins.Add(new FoldingMargin(ed));
            ed.RightMargins.Add(new ScrollBarMargin(ed, Orientation.Vertical));
            ed.BottomMargins.Add(new ScrollBarMargin(ed, Orientation.Horizontal));
            ed.TopMargins.Add(new TopMargin(ed));
            ed.Styles.Default.ForeColor = ColorTranslator.FromHtml("#DCDCDC");
            ed.Styles.Default.BackColor = ColorTranslator.FromHtml("#1E1E1E");
            ed.Styles.SpecialSymbol.ForeColor = ColorTranslator.FromHtml("#505050");

            ed.Styles.Selection.BackColor = ColorTranslator.FromHtml("#264F78");
            ed.Styles.MatchBracket.BackColor = ColorTranslator.FromHtml("#264F78");

            ed.Styles.Number.ForeColor = ColorTranslator.FromHtml("#B5CEA8");
            ed.Styles.Bracket.ForeColor = ColorTranslator.FromHtml("#A5A5A5");

            ed.Styles.Keyword.ForeColor = HCol("#569CD6");
            ed.Styles.KeywordSpecial.ForeColor = HCol("#8CDCDB");
            ed.Styles.TypeName.ForeColor = HCol("#44C7AE");
            ed.Styles.Literal.ForeColor = HCol("#B8D7A3");
            ed.Styles.Comment.ForeColor = HCol("#579032");
            ed.Styles.CommentMultiline.ForeColor = HCol("#579032");
            ed.Styles.CommentDocument.ForeColor = HCol("#579032");
            ed.Styles.Char.ForeColor = HCol("#D69D85");
            ed.Styles.String.ForeColor = HCol("#D69D85");
            ed.Styles.StringMultiline.ForeColor = HCol("#D69D85");
            ed.Styles.StringSplice.ForeColor = HCol("#D69D85");
            ed.Styles.Regex.ForeColor = HCol("#FF7CDC");
            ed.Styles.Preprocessor.ForeColor = HCol("#7F7F7F");

            ed.Settings.LineNumbersForeColor = ColorTranslator.FromHtml("#505050");
            ed.Settings.LineNumbersBackColor = ColorTranslator.FromHtml("#1E1E1E");
            ed.Settings.LineNumbersCurrentForeColor = ColorTranslator.FromHtml("#848484");
            ed.Settings.LineNumbersCurrentBackColor = ColorTranslator.FromHtml("#262626");

            ed.Settings.FoldingBackColor = ColorTranslator.FromHtml("#1E1E1E");
            ed.Settings.FoldingForeColor = ColorTranslator.FromHtml("#505050");
            ed.Settings.FoldingActiveForeColor = ColorTranslator.FromHtml("#C0AAF7");

            ed.Settings.PopupForeColor = ColorTranslator.FromHtml("#DCDCDC");
            ed.Settings.PopupBackColor = ColorTranslator.FromHtml("#2D2D30");
            ed.Settings.PopupHoverColor = ColorTranslator.FromHtml("#3E3E42");
            ed.Settings.PopupSelectedColor = ColorTranslator.FromHtml("#264F78");
            ed.Settings.PopupBorderColor = ColorTranslator.FromHtml("#5F5F66");

            ed.Text = File.ReadAllText(//@"C:\Test\bigcode.cs");
                LocalFile("test.htm"));
            ed.Commands.ReadKeymap(LocalFile("samples\\keymap.json"));
        }

        private string LocalFile(string fileName)
        {
            return Path.Combine(new FileInfo(typeof(MainForm).Assembly.Location).DirectoryName, fileName);
        }
        
        private void BindCommands()
        {
            //ed.Commands.Bind("command.editor.togglefolding", Keys.Control | Keys.K);
            //ed.Commands.Bind("command.editor.autocomplete", Keys.Control | Keys.Space);
            //ed.Commands.Bind("command.editor.followlink", MouseEvents.Click, Keys.Control);
            //ed.Commands.Bind("command.editor.deletewordback", Keys.Control | Keys.Back);
            //ed.Commands.Bind("command.editor.deleteword", Keys.Control | Keys.Delete);
            //ed.Commands.Bind("command.editor.scrollup", Keys.Control | Keys.Up);
            //ed.Commands.Bind("command.editor.scrolldown", Keys.Control | Keys.Down);
            //ed.Commands.Bind("command.editor.redo", Keys.Control | Keys.Y);
            //ed.Commands.Bind("command.editor.undo", Keys.Control | Keys.Z);
            //ed.Commands.Bind("command.editor.selectall", Keys.Control | Keys.A);
            //ed.Commands.Bind("command.editor.paste", Keys.Control | Keys.V);
            //ed.Commands.Bind("command.editor.cut", Keys.Control | Keys.X);
            //ed.Commands.Bind("command.editor.copy", Keys.Control | Keys.C);
            //ed.Commands.Bind("command.editor.caretset", MouseEvents.Click, Keys.Alt);
            //ed.Commands.Bind("command.editor.caretset", MouseEvents.Click, Keys.None);
            //ed.Commands.Bind("command.editor.caretadd", MouseEvents.Click, Keys.Control);
            //ed.Commands.Bind("command.editor.selectnormal", MouseEvents.Move | MouseEvents.Click, Keys.Control);
            //ed.Commands.Bind("command.editor.selectnormal", MouseEvents.Move | MouseEvents.Click, Keys.None);
            //ed.Commands.Bind("command.editor.selectblock", MouseEvents.Move | MouseEvents.Click, Keys.Alt);
            //ed.Commands.Bind("command.editor.selectword", MouseEvents.DoubleClick, Keys.None);
            //ed.Commands.Bind("command.editor.unindent", Keys.Shift | Keys.Tab);
            //ed.Commands.Bind("command.editor.indent", Keys.Tab);
            //ed.Commands.Bind("command.editor.selectionclear", Keys.Escape);
            //ed.Commands.Bind("command.editor.left", Keys.Left);
            //ed.Commands.Bind("command.editor.right", Keys.Right);
            //ed.Commands.Bind("command.editor.up", Keys.Up);
            //ed.Commands.Bind("command.editor.down", Keys.Down);
            //ed.Commands.Bind("command.editor.home", Keys.Home);
            //ed.Commands.Bind("command.editor.end", Keys.End);
            //ed.Commands.Bind("command.editor.newline", Keys.Enter);
            //ed.Commands.Bind("command.editor.deleteback", Keys.Back);
            //ed.Commands.Bind("command.editor.delete", Keys.Delete);
            //ed.Commands.Bind("command.editor.pagedown", Keys.PageDown);
            //ed.Commands.Bind("command.editor.pageup", Keys.PageUp);
            //ed.Commands.Bind("command.editor.extendleft", Keys.Shift | Keys.Left);
            //ed.Commands.Bind("command.editor.extendright", Keys.Shift | Keys.Right);
            //ed.Commands.Bind("command.editor.extendup", Keys.Shift | Keys.Up);
            //ed.Commands.Bind("command.editor.extenddown", Keys.Shift | Keys.Down);
            //ed.Commands.Bind("command.editor.extendend", Keys.Shift | Keys.End);
            //ed.Commands.Bind("command.editor.extendhome", Keys.Shift | Keys.Home);
            //ed.Commands.Bind("command.editor.wordleft", Keys.Control | Keys.Left);
            //ed.Commands.Bind("command.editor.wordright", Keys.Control | Keys.Right);
            //ed.Commands.Bind("command.editor.extendwordright", Keys.Control | Keys.Shift | Keys.Right);
            //ed.Commands.Bind("command.editor.extendwordleft", Keys.Control | Keys.Shift | Keys.Left);
            //ed.Commands.Bind("command.editor.extendpagedown", Keys.Shift | Keys.PageDown);
            //ed.Commands.Bind("command.editor.extendpageup", Keys.Shift | Keys.PageUp);
            //ed.Commands.Bind("command.editor.documenthome", Keys.Control | Keys.Home);
            //ed.Commands.Bind("command.editor.documentend", Keys.Control | Keys.End);
            //ed.Commands.Bind("command.editor.extenddocumenthome", Keys.Control | Keys.Shift | Keys.Home);
            //ed.Commands.Bind("command.editor.extenddocumentend", Keys.Control | Keys.Shift | Keys.End);
            //ed.Commands.Bind("command.editor.overtype", Keys.Insert);
        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            ed.Focus();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            Focus();
        }
        
    }
}
