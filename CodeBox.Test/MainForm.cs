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

            var lexer = new ConfigurableLexer();
            //lexer.GrammarProvider.RegisterGrammar(HtmlGrammar2());
            //lexer.GrammarProvider.RegisterGrammar(CsGrammar());
            //lexer.GrammarProvider.RegisterGrammar(CssGrammar());
            //lexer.GrammarKey = "html";
            ed.Styles.Provider = lexer;
            var csharp = GrammarReader.Read(File.ReadAllText(LocalFile("grammars\\csharp.grammar.json")));
            var csharpExp = GrammarReader.Read(File.ReadAllText(LocalFile("grammars\\csharp-expression.grammar.json")));
            var html = GrammarReader.Read(File.ReadAllText(LocalFile("grammars\\html.grammar.json")));
            lexer.GrammarProvider.RegisterGrammar(csharp);
            lexer.GrammarProvider.RegisterGrammar(csharpExp);
            lexer.GrammarProvider.RegisterGrammar(html);
            lexer.GrammarKey = "html";


            ed.Styles.StyleNeeded += editor1_StyleNeeded;
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


      
        private Grammar HtmlGrammar2()
        {
            var grm = new Grammar
            {
                Key = "html",
                NonWordSymbols = "`~!@#$%^&*()=+[{]}\\|;'\",<>/?",
                BracketSymbols = "<>",
                //IndentProvider = new BlockDentProvider()
            };
            grm.AddSection(new GrammarSection
            {
                Id = 0
            });
            grm.AddSection(new GrammarSection
            {
                Id = 1,
                Start = new SectionSequence("<", true),
                End = new SectionSequence(">", true),
                Multiline = true,
                ContextChars = "</",
                ContextIdentifierStyle = StandardStyle.Keyword,
                IdentifierStyle = StandardStyle.KeywordSpecial
            });
            grm.AddSection(new GrammarSection
            {
                Id = 2,
                ParentId = 1,
                Start = new SectionSequence("\"", true),
                End = new SectionSequence("\"", true),
                Style = StandardStyle.String
            });
            grm.AddSection(new GrammarSection
            {
                Id = 3,
                ParentId = 0,
                Start = new SectionSequence("<script >", false),
                End = new SectionSequence("</script>", false),
                Multiline = true,
                DontStyleCompletely = true,
                ExternalGrammarKey = "csharp"
            });
            grm.AddSection(new GrammarSection
            {
                Id = 4,
                Start = new SectionSequence("<style >", false),
                End = new SectionSequence("</style>", false),
                Multiline = true,
                DontStyleCompletely = true,
                ExternalGrammarKey = "css"
            });
            grm.AddSection(new GrammarSection
            {
                Id = 5,
                Start = new SectionSequence("<!", true),
                End = new SectionSequence(">", true),
                Multiline = true,
                Style = (int)StandardStyle.Default
            });
            grm.AddSection(new GrammarSection
            {
                Id = 6,
                Start = new SectionSequence("<!--", true),
                End = new SectionSequence("-->", true),
                Multiline = true,
                Style = StandardStyle.CommentMultiline
            });




            return grm;
        }


        private Grammar HtmlGrammar()
        {
            var grm = new Grammar
            {
                Key = "html",
                NonWordSymbols = "`~!@#$%^&*()=+[{]}\\|;'\",<>/?",
                BracketSymbols = "<>",
                //IndentProvider = new BlockDentProvider()
            };

            grm.AddSection(new GrammarSection
            {
            }); //root
            grm.AddSection(new GrammarSection
            {
                Id = 1,
                Start = new SectionSequence("<!", true),
                End = new SectionSequence(">", true),
                Multiline = true,
                Style = (int)StandardStyle.Default
            });
            var msect = new GrammarSection
            {
                Id = 2,
                Start = new SectionSequence("<", true),
                End = new SectionSequence(">", true),
                Multiline = true,
                IdentifierStyle = StandardStyle.KeywordSpecial,
                ContextIdentifierStyle = StandardStyle.Keyword,
                ContextChars = "<"
            };
            msect.Keywords.Add("script", ((int)StandardStyle.Keyword & 0xFFFF) | (42 << 16));
            msect.Keywords.Add("style", ((int)StandardStyle.Keyword & 0xFFFF) | (84 << 16));
            grm.AddSection(msect);
            

            grm.AddSection(new GrammarSection
            {
                Id = 3,
                Start = new SectionSequence("</", true),
                End = new SectionSequence(">", true),
                IdentifierStyle = StandardStyle.Keyword,
                Style = 0
            });
            grm.AddSection(new GrammarSection
            {
                Id = 4,
                //StartKeyword = 42,
                End = new SectionSequence("</script>", false),
                Multiline = true,
                DontStyleCompletely = true,
                ExternalGrammarKey = "csharp"
            });
            grm.AddSection(new GrammarSection
            {
                Id = 5,
                Start = new SectionSequence("<!--", true),
                End = new SectionSequence("-->", true),
                Multiline = true,
                Style = StandardStyle.CommentMultiline
            });
            grm.AddSection(new GrammarSection
            {
                Id = 6,
                ParentId = 2,
                Start = new SectionSequence("\"", true),
                End = new SectionSequence("\"", true),
                Style = StandardStyle.String
            });
            grm.AddSection(new GrammarSection
            {
                Id = 7,
                //StartKeyword = 84,
                End = new SectionSequence("</style>", false),
                Multiline = true,
                DontStyleCompletely = true,
                ExternalGrammarKey = "css"
            });




            return grm;
        }

        private void BindCommands()
        {
            ed.Commands.Bind<ToggleFoldingCommand>(Keys.Control | Keys.K);
            ed.Commands.Bind<AutocompleteCommand>(Keys.Control | Keys.Space);
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
        
        private void editor1_StyleNeeded(object sender, StyleNeededEventArgs e)
        {
            var state = ed.Document.Lines[e.Range.Start.Line].State;

            for (var i = e.Range.Start.Line; i < e.Range.End.Line + 1; i++)
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
                    var ei = ParseString(line, i + 1, str);
                    var st = FindSemi(ei + 1, str);
                    ed.Styles.StyleRange(st ? 110 : 111, line, i, ei);
                    i = ei + 1;
                }
                else if (c == '/' && i < str.Length - 1 && str[i + 1] == '*')
                {
                    bool end;
                    var ei = ParseComment(i + 1, str, out end);
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

        private int ParseString(int line, int i, string str)
        {
            var init = i;
            
            for (; i < str.Length; i++)
            {
                var c = str[i];

                if (c == '"' && (str[i - 1] != '\\' || (i > 1 && str[i - 2] == '\\')))
                    return i;
                else if (c == ':' && i > init && i < str.Length - 3 && str[i + 1] == '/' && str[i + 2] == '/')
                {
                    var i1 = LookBackForSpace(i, str);
                    var i2 = LookUpForSpace(i, str);

                    if (i1 != -1 && i2 != -1)
                    {
                        ed.Styles.StyleRange((int)StandardStyle.Hyperlink, line, i1, i2);
                        ed.CallTips.BindCallTip("Ctrl + Click to follow link", new Pos(line, i1), new Pos(line, i2));
                    }
                }
            }

            return str.Length - 1;
        }

        private int LookUpForSpace(int i, string str)
        {
            for (var j = i; i < str.Length; i++)
                if (str[i] == ' ' || str[i] == '"')
                    return i - 1;

            return -1;
        }

        private int LookBackForSpace(int i, string str)
        {
            for (var j = i; i > -1; i--)
                if (str[i] == ' ' || str[i] == '"')
                    return i + 1;

            return -1;
        }
    }
}
