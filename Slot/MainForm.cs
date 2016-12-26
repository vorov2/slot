using System;
using System.IO;
using System.Windows.Forms;
using Slot.Core;
using Slot.Core.State;
using Slot.Core.Themes;
using Slot.Core.ViewModel;
using Slot.Core.Workspaces;
using Slot.Editor;
using Slot.Editor.Margins;
using Slot.Main.CommandBar;
using Slot.Main.StatusBar;

namespace Slot
{
    public partial class MainForm : Form
    {
        private EditorControl ed;
        //private EditorControl output;
        private CommandBarControl commandBar;
        private readonly static Guid stateId = new Guid("91B8C085-429A-4460-94FB-8B320DFB0DDA");

        public MainForm()
        {
            InitializeComponent();
            Initialize();
            //InitializeOutput(ed.Settings);
        }

        private void ReadState()
        {
            App.Ext.Handle(() =>
            {
                var stream = App.Catalog<IStateManager>().Default().ReadState(stateId);

                if (stream != null)
                    using (var br = new BinaryReader(stream))
                    {
                        Top = br.ReadInt32();
                        Left = br.ReadInt32();
                        Width = br.ReadInt32();
                        Height = br.ReadInt32();
                        App.Catalog<IThemeComponent>().Default().ChangeTheme((Identifier)br.ReadString());
                    }
            });
        }

        private void WriteState()
        {
            if (WindowState != FormWindowState.Normal)
                return;

            App.Ext.Handle(() =>
            {
                var stream = App.Catalog<IStateManager>().Default().WriteState(stateId);

                if (stream != null)
                    using (var bw = new BinaryWriter(stream))
                    {
                        bw.Write(Top);
                        bw.Write(Left);
                        bw.Write(Width);
                        bw.Write(Height);
                        bw.Write(App.Catalog<IThemeComponent>().Default().Theme.Key.ToString());
                    }
            });
        }

        private void Initialize()
        {
            ReadState();
            ed = new EditorControl { Dock = DockStyle.Fill };

            commandBar = new CommandBarControl(ed) { Dock = DockStyle.Top };


            ed.LeftMargins.Add(new LineNumberMargin(ed) { MarkCurrentLine = true });
            ed.LeftMargins.Add(new FoldingMargin(ed));
            ed.RightMargins.Add(new VerticalScrollBarMargin(ed));
            ed.BottomMargins.Add(new ScrollBarMargin(ed, Orientation.Horizontal));
            ed.TopMargins.Add(new TopMargin(ed));
            var statusBar = new StatusBarControl(ed) { Dock = DockStyle.Bottom };
            statusBar.Tiles.Add(new HelpTile());
            statusBar.Tiles.Add(new ModeTile(ed));
            statusBar.Tiles.Add(new EolTile(ed));
            statusBar.Tiles.Add(new EncodingTile(ed));

            //statusBar.Tiles.Add(new OutputToggleTile(this));
            statusBar.Tiles.Add(new PosTile(ed));
            //statusBar.Tiles.Add(new ErrorsTile(ed));
            statusBar.Tiles.Add(new OvrTile(ed));
            statusBar.Tiles.Add(new WrapTile());


            splitContainer.Panel1.Controls.Add(ed);
            splitContainer.Panel1.Controls.Add(commandBar);
            splitContainer.Panel1.Controls.Add(statusBar);
            //statusBar.BringToFront();
            ed.Paint += (o, e) =>
            {
                statusBar.Invalidate();
                commandBar.Invalidate();
            };
        }

        private void InitializeOutput(EditorSettings set)
        {
            //output = new EditorControl { Dock = DockStyle.Fill };
            //output.LeftMargins.Add(new GutterMargin(output));
            //output.RightMargins.Add(new ScrollBarMargin(output, Orientation.Vertical));
            //output.BottomMargins.Add(new ScrollBarMargin(output, Orientation.Horizontal));
            //output.ThinCaret = true;

            ////output.Settings.Font = set.SmallFont;
            ////output.Settings.ShowEol = output.Settings.ShowLineLength = false;
            ////output.Settings.ShowWhitespace = ShowWhitespace.None;
            //output.LimitedMode = true;
            //output.AttachBuffer(App.Catalog<ILogComponent>().GetComponent((Identifier)"log.application"));
            //output.ReadOnly = true;

            //splitContainer.Panel2.Controls.Add(output);
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
            {
                ed.DetachBuffer();
                WriteState();
            }
        }
    }
}
