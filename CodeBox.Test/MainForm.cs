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
        }

        private Color HCol(string str)
        {
            return ColorTranslator.FromHtml(str);
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
        }

        private string LocalFile(string fileName)
        {
            return Path.Combine(new FileInfo(typeof(MainForm).Assembly.Location).DirectoryName, fileName);
        }
        
        private void BindCommands()
        {
            ed.Commands.Bind<ToggleFoldingCommand>(Keys.Control | Keys.K);
            ed.Commands.Bind<AutocompleteCommand>(Keys.Control | Keys.Space);
            ed.Commands.Bind<FollowLinkCommand>(MouseEvents.Click, Keys.Control);
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
            ed.Commands.Bind<ExtendPageDownCommand>(Keys.Shift | Keys.PageDown);
            ed.Commands.Bind<ExtendPageUpCommand>(Keys.Shift | Keys.PageUp);
            ed.Commands.Bind<DocumentHomeCommand>(Keys.Control | Keys.Home);
            ed.Commands.Bind<DocumentEndCommand>(Keys.Control | Keys.End);
            ed.Commands.Bind<ExtendDocumentHomeCommand>(Keys.Control | Keys.Shift | Keys.Home);
            ed.Commands.Bind<ExtendDocumentEndCommand>(Keys.Control | Keys.Shift | Keys.End);
            ed.Commands.Bind<OvertypeCommand>(Keys.Insert);
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
