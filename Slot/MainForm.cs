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
using Slot.Editor.Margins;
using Slot.Editor.ObjectModel;
using Slot.Editor.Styling;
using Slot.Editor.Commands;
using Slot.Editor.Folding;
using Slot.Editor.Lexing;
using Slot.Editor.Indentation;
using Slot.Editor.Affinity;
using Slot.Core.Keyboard;
using Slot.Editor;
using Slot.Core;
using Slot.Core.CommandModel;
using Slot.Core.ComponentModel;
using Slot.ComponentModel;
using Slot.Main.StatusBar;
using Slot.Core.Output;
using Slot.Core.Settings;
using Slot.Core.Workspaces;
using Slot.Main.CommandBar;
using Slot.Core.ViewModel;

namespace Slot
{
    public partial class MainForm : Form
    {
        private EditorControl ed;
        private EditorControl output;
        private CommandBarControl commandBar;

        public MainForm()
        {
            InitializeComponent();
            Initialize();
            InitializeOutput(ed.Settings);
        }

        private void Initialize()
        {
            ed = new EditorControl { Dock = DockStyle.Fill };

            commandBar = new CommandBarControl(ed) { Dock = DockStyle.Top };


            ed.LeftMargins.Add(new LineNumberMargin(ed) { MarkCurrentLine = true });
            ed.LeftMargins.Add(new FoldingMargin(ed));
            ed.RightMargins.Add(new VerticalScrollBarMargin(ed));
            ed.BottomMargins.Add(new ScrollBarMargin(ed, Orientation.Horizontal));
            ed.TopMargins.Add(new TopMargin(ed));
            var statusBar = new StatusBarControl(ed) { Dock = DockStyle.Bottom };
            statusBar.Tiles.Add(new HelpTile(ed));
            statusBar.Tiles.Add(new ModeTile(ed));
            statusBar.Tiles.Add(new EolTile(ed));
            statusBar.Tiles.Add(new EncodingTile(ed));

            statusBar.Tiles.Add(new OutputToggleTile(this));
            statusBar.Tiles.Add(new PosTile(ed));
            //statusBar.Tiles.Add(new ErrorsTile(ed));
            statusBar.Tiles.Add(new OvrTile(ed));
            statusBar.Tiles.Add(new WrapTile(ed));


            splitContainer.Panel1.Controls.Add(ed);
            splitContainer.Panel1.Controls.Add(commandBar);
            splitContainer.Panel1.Controls.Add(statusBar);
            //statusBar.BringToFront();
            ed.Paint += (o, e) =>
            {
                statusBar.Invalidate();
                commandBar.Invalidate();
            };
            //var coll = StylesReader.Read(File.ReadAllText("samples\\theme2.json"));
            //ed.Styles.Theme = coll;
        }

        private void InitializeOutput(EditorSettings set)
        {
            output = new EditorControl { Dock = DockStyle.Fill };
            output.LeftMargins.Add(new GutterMargin(output));
            output.RightMargins.Add(new ScrollBarMargin(output, Orientation.Vertical));
            output.BottomMargins.Add(new ScrollBarMargin(output, Orientation.Horizontal));
            output.ThinCaret = true;

            //output.Settings.Font = set.SmallFont;
            //output.Settings.ShowEol = output.Settings.ShowLineLength = false;
            //output.Settings.ShowWhitespace = ShowWhitespace.None;
            output.LimitedMode = true;
            output.AttachBuffer(App.Catalog<ILogComponent>().GetComponent((Identifier)"log.application"));
            output.ReadOnly = true;

            splitContainer.Panel2.Controls.Add(output);
        }

        //protected override CreateParams CreateParams
        //{
        //    get
        //    {
        //        CreateParams cp = base.CreateParams;
        //        cp.ExStyle |= 0x02000000;   // WS_EX_COMPOSITED
        //        return cp;
        //    }
        //}



        public SplitContainer SplitContainer => splitContainer;

        private string LocalFile(string fileName)
        {
            return Path.Combine(new FileInfo(typeof(MainForm).Assembly.Location).DirectoryName, fileName);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            var wc = App.Catalog<IWorkspaceController>().Default();
            wc.WorkspaceChanged += (o, ev) => UpdateTitle();
        }

        internal void UpdateTitle()
        {
            var wm = App.Catalog<IViewManager>().Default()?.GetActiveView();
            Text = wm.Workspace == null ? Application.ProductName
                : $"{wm.Workspace.FullName} - {Application.ProductName}";
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

        public EditorControl Editor => ed;

        public int Activations { get; private set; }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (App.Terminating || Application.OpenForms.Count > 1 || !(e.Cancel = !App.Close()))
                ed.DetachBuffer();
        }
    }
}
