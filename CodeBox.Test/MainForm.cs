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
using CodeBox.Core.Keyboard;
using CodeBox.Core;
using CodeBox.Core.CommandModel;
using CodeBox.CommandLine;
using CodeBox.Core.ComponentModel;
using CodeBox.ComponentModel;

namespace CodeBox.Test
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            base.KeyPreview = true;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ed.Styles.StylerKey = (Identifier)"styler.default";
            var csharp = GrammarReader.Read(File.ReadAllText(LocalFile("grammars\\csharp.grammar.json")));
            var csharpExp = GrammarReader.Read(File.ReadAllText(LocalFile("grammars\\csharp-expression.grammar.json")));
            var html = GrammarReader.Read(File.ReadAllText(LocalFile("grammars\\html.grammar.json")));
            var css = GrammarReader.Read(File.ReadAllText(LocalFile("grammars\\css.grammar.json")));
            var json = GrammarReader.Read(File.ReadAllText(LocalFile("grammars\\json.grammar.json")));
            var grm = ComponentCatalog.Instance.Grammars();
            grm.RegisterGrammar(csharp);
            grm.RegisterGrammar(csharpExp);
            grm.RegisterGrammar(html);
            grm.RegisterGrammar(css);
            grm.RegisterGrammar(json);

            CommandCatalog.Instance.RegisterCommands(CommandReader.Read(File.ReadAllText(LocalFile("samples\\commands.json"))));


            ed.LeftMargins.Add(new LineNumberMargin(ed) { MarkCurrentLine = true });
            ed.LeftMargins.Add(new FoldingMargin(ed));
            ed.RightMargins.Add(new ScrollBarMargin(ed, Orientation.Vertical));
            ed.BottomMargins.Add(new ScrollBarMargin(ed, Orientation.Horizontal));
            ed.TopMargins.Add(new CommandMargin(ed));
            ed.TopMargins.Add(new TopMargin(ed));

            //var coll = StylesReader.Read(File.ReadAllText("samples\\theme2.json"));
            //ed.Styles.Theme = coll;
            SettingsReader.Read(File.ReadAllText("samples\\settings.json"), ed);
            KeymapReader.Read(File.ReadAllText(LocalFile("samples\\keymap.json")), KeyboardAdapter.Instance);

            var theme = ComponentCatalog.Instance.GetComponent((Identifier)"theme.default") as IThemeComponent;
            theme.ChangeTheme("dark");

            var fl = LocalFile("test.htm");//@"c:\test\bigcode.cs";//
            ed.AttachBuffer(new DocumentBuffer(Document.FromString(File.ReadAllText(fl)), fl, Encoding.UTF8));
        }

        private string LocalFile(string fileName)
        {
            return Path.Combine(new FileInfo(typeof(MainForm).Assembly.Location).DirectoryName, fileName);
        }
        

        private void Form1_Activated(object sender, EventArgs e)
        {
            //ed.Focus();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            //Focus();
        }
    }
}
