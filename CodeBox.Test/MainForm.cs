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
            Initialize();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
        }

        private void Initialize()
        {
            CommandCatalog.Instance.RegisterCommands(CommandReader.Read(File.ReadAllText(LocalFile("samples\\commands.json"))));
            SettingsReader.Read(File.ReadAllText("samples\\settings.json"), ed.Settings);

            if (ed.Settings.ShowLineNumbers)
                ed.LeftMargins.Add(new LineNumberMargin(ed) { MarkCurrentLine = true });

            ed.LeftMargins.Add(new FoldingMargin(ed));
            ed.RightMargins.Add(new ScrollBarMargin(ed, Orientation.Vertical));
            ed.BottomMargins.Add(new ScrollBarMargin(ed, Orientation.Horizontal));
            ed.TopMargins.Add(new CommandMargin(ed));
            ed.TopMargins.Add(new TopMargin(ed));

            
            //var coll = StylesReader.Read(File.ReadAllText("samples\\theme2.json"));
            //ed.Styles.Theme = coll;
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

        public Editor Editor => ed;
    }
}
