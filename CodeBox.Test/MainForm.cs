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
using CodeBox.StatusBar;

namespace CodeBox.Test
{
    public partial class MainForm : Form
    {
        private Editor ed;

        public MainForm()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            ed = new Editor { Dock = DockStyle.Fill };
            SettingsReader.Read(File.ReadAllText("samples\\settings.json"), ed.Settings);

            if (ed.Settings.ShowLineNumbers)
                ed.LeftMargins.Add(new LineNumberMargin(ed) { MarkCurrentLine = true });

            ed.LeftMargins.Add(new FoldingMargin(ed));
            ed.RightMargins.Add(new ScrollBarMargin(ed, Orientation.Vertical));
            ed.BottomMargins.Add(new ScrollBarMargin(ed, Orientation.Horizontal));
            ed.TopMargins.Add(new CommandMargin(ed));
            ed.TopMargins.Add(new TopMargin(ed));

            var statusBar = new StatusBarControl(ed) { Dock = DockStyle.Bottom };
            var grm = ComponentCatalog.Instance.GetComponent((Identifier)"grammar.default") as IGrammarComponent;
            statusBar.Tiles.Add(new ModeTile(ed));
            statusBar.Tiles.Add(new LineEndingTile(ed));
            statusBar.Tiles.Add(new EncodingTile(ed));
            statusBar.Tiles.Add(new PosTile(ed));
            statusBar.Tiles.Add(new OvrTile(ed));


            splitContainer.Panel1.Controls.Add(ed);
            splitContainer.Panel1.Controls.Add(statusBar);
            //statusBar.BringToFront();
            ed.Paint += (o, e) => statusBar.Invalidate();
            //var coll = StylesReader.Read(File.ReadAllText("samples\\theme2.json"));
            //ed.Styles.Theme = coll;
        }

        private string LocalFile(string fileName)
        {
            return Path.Combine(new FileInfo(typeof(MainForm).Assembly.Location).DirectoryName, fileName);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }


        private void Form1_Activated(object sender, EventArgs e)
        {
            Activations++;
            //ed.Focus();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            //Focus();
        }

        public Editor Editor => ed;

        public int Activations { get; private set; }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (App.Instance.Terminating || Application.OpenForms.Count > 1 || !(e.Cancel = !App.Instance.Close()))
                ed.DetachBuffer();
        }
    }
}
