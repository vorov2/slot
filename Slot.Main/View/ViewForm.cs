using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Slot.Core;
using Slot.Core.Settings;
using Slot.Core.Themes;
using Slot.Core.ViewModel;
using Slot.Drawing;
using Slot.Editor;
using Slot.Editor.ObjectModel;
using Slot.Main.CommandBar;
using Slot.Main.Notifications;
using Slot.Main.StatusBar;

namespace Slot.Main.View
{
    internal sealed partial class ViewForm : Form, IView
    {
        private readonly object syncRoot = new object();
        private readonly StandardEditor editor;
        private readonly StatusBarControl statusBar;
        private readonly CommandBarControl commandBar;
        private readonly HeaderControl header;

        public ViewForm()
        {
            editor = new StandardEditor(Settings.Get<EditorSettings>());
            statusBar = new StatusBarControl(editor);
            commandBar = new CommandBarControl(editor);
            header = new HeaderControl(editor);
            Initialize();

            if (!ReadState())
            {
                Width = 800 * Dpi.GetWidth(1);
                Height = 600 * Dpi.GetWidth(1);
            }
        }

        private void Initialize()
        {
            statusBar.Tiles.Add(new HelpTile());
            statusBar.Tiles.Add(new ModeTile(editor));
            statusBar.Tiles.Add(new EolTile(editor));
            statusBar.Tiles.Add(new EncodingTile(editor));
            statusBar.Tiles.Add(new PosTile(editor));
            statusBar.Tiles.Add(new OvrTile(editor));
            statusBar.Tiles.Add(new WrapTile());
            editor.AddSlave(statusBar);
            editor.AddSlave(header);
            Controls.Add(editor);
            Controls.Add(statusBar);
            Controls.Add(header);
            commandBar.Visible = false;
            Controls.Add(commandBar);
            commandBar.BringToFront();
            Icon = new Icon(typeof(ViewForm).Assembly.GetManifestResourceStream("Slot.Main.Properties.app.ico"));
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (!AllowClose)
            {
                e.Cancel = true;
                App.Component<IViewManager>().CloseView(this);
            }
            else
            {
                WriteState();
                var buf = (DocumentBuffer)Buffer;

                if (buf.RefCount == 1)
                {
                    App.Ext.Run(Cmd.CloseFile);

                    if (buf.RefCount > 0)
                        e.Cancel = true;
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            BackColor = App.Component<ITheme>().GetStyle(StandardStyle.Default).BackColor;
            base.OnPaint(e);
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            LastAccess = DateTime.Now;

            if (Buffer != null)
                Buffer.LastAccess = LastAccess;
        }

        private void UpdateTitle()
        {
            Text = Workspace == null || (Buffer != null && Buffer.Flags.HasFlag(BufferDisplayFlags.HideWorkspace))
                ? Application.ProductName
                : $"{Workspace.FullName} - {Application.ProductName}";
        }

        public void AttachBuffer(IBuffer buffer)
        {
            editor.AttachBuffer(buffer);
            header.Visible = !buffer.Flags.HasFlag(BufferDisplayFlags.HideHeader);
            statusBar.Visible = !buffer.Flags.HasFlag(BufferDisplayFlags.HideStatusBar);
            UpdateTitle();
            Invalidate(true);
        }

        private bool _allowClose;
        public bool AllowClose
        {
            get { return _allowClose || App.Terminating; }
            set { _allowClose = value; }
        }

        public void DetachBuffer() => editor.DetachBuffer();

        public object Header => header;

        public object CommandBar => commandBar;

        public IEditor Editor => editor;

        public IBuffer Buffer => editor.Buffer;

        public DateTime LastAccess { get; set; }

        private ISettings _settings;
        public ISettings Settings
        {
            get
            {
                if (_settings == null)
                    lock (syncRoot)
                        if (_settings == null)
                            _settings = App.Component<ISettingsManager>().Create(this);

                return _settings;
            }
        }

        public Identifier Mode
        {
            get { return editor.Buffer?.Mode; }
            set { editor.Buffer.Mode = value; }
        }

        private DirectoryInfo _workspace;
        public DirectoryInfo Workspace
        {
            get { return _workspace; }
            set
            {
                if (value != _workspace)
                {
                    _workspace = value;
                    UpdateTitle();
                }
            }
        }
    }
}
