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
            ed.TopMargins.Add(new CommandMargin(ed));
            ed.TopMargins.Add(new TopMargin(ed));

            var coll = StylesReader.Read(File.ReadAllText("samples\\theme.json"));
            ed.Styles.Styles = coll;
            SettingsReader.Read(File.ReadAllText("samples\\settings.json"), ed);
            KeymapReader.Read(File.ReadAllText(LocalFile("samples\\keymap.json")), ed.KeyboardAdapter);
            ed.Text = File.ReadAllText(//@"C:\Test\bigcode.cs");
                LocalFile("test.htm"));
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
