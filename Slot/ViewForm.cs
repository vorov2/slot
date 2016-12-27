﻿using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Slot.Core;
using Slot.Core.Settings;
using Slot.Core.ViewModel;
using Slot.Core.Workspaces;
using Slot.Drawing;
using Slot.Editor;
using Slot.Main.CommandBar;
using Slot.Main.StatusBar;

namespace Slot
{
    public sealed partial class ViewForm : Form, IView
    {
        private readonly object syncRoot = new object();
        private readonly StandardEditor editor;
        private readonly StatusBarControl statusBar;
        private readonly CommandBarControl commandBar;

        public ViewForm()
        {
            editor = new StandardEditor(Settings.Get<EditorSettings>());
            statusBar = new StatusBarControl(editor);
            commandBar = new CommandBarControl(editor);
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
            editor.AddSlave(commandBar);
            Controls.Add(editor);
            Controls.Add(statusBar);
            Controls.Add(commandBar);
            Icon = new Icon(typeof(ViewForm).Assembly.GetManifestResourceStream("Slot.Properties.app.ico"));
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (!AllowClose)
            {
                e.Cancel = true;
                App.Catalog<IViewManager>().Default().CloseView(this);
            }
            else
            {
                DetachBuffer();
                WriteState();
            }
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
            Text = Workspace == null ? Application.ProductName
                : $"{Workspace.FullName} - {Application.ProductName}";
        }

        public void AttachBuffer(IBuffer buffer)
        {
            editor.AttachBuffer(buffer);
            commandBar.Visible = !buffer.Flags.HasFlag(BufferDisplayFlags.HideCommandBar);
            statusBar.Visible = !buffer.Flags.HasFlag(BufferDisplayFlags.HideStatusBar);
            Invalidate(true);
        }

        private bool _allowClose;
        public bool AllowClose
        {
            get { return _allowClose || App.Terminating; }
            set { _allowClose = value; }
        }

        public void DetachBuffer() => editor.DetachBuffer();

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
                            _settings = App.Catalog<ISettingsManager>().Default().Create(this);

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
