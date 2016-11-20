using CodeBox.Commands;
using CodeBox.Core.Keyboard;
using CodeBox.Lexing;
using CodeBox.Margins;
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

namespace CodeBox.Test
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
            InitializeEditor();
        }

        private void InitializeEditor()
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
            ed.TopMargins.Add(new TopMargin(ed));

            var coll = Styling.StylesReader.Read(File.ReadAllText("samples\\theme2.json"));
            ed.Styles.Styles = coll;
            SettingsReader.Read(File.ReadAllText("samples\\settings.json"), ed);
            KeymapReader.Read(File.ReadAllText(LocalFile("samples\\keymap.json")), ed.KeyboardAdapter);
            ed.Text = File.ReadAllText(LocalFile("test.htm"));
        }

        private string LocalFile(string fileName)
        {
            return Path.Combine(new FileInfo(typeof(MainForm).Assembly.Location).DirectoryName, fileName);
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            var cmdEd = new Editor(ed.Settings, ed.Styles.Styles, ed.KeyboardAdapter);
            cmd.CommandEditor = cmdEd;

            
        }
    }
}
