using Slot.Core;
using Slot.Core.CommandModel;
using Slot.Core.ComponentModel;
using Slot.Core.Output;
using Slot.Core.ViewModel;
using Slot.Core.Workspaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Slot.Main.File
{
    [Export(typeof(ICommandDispatcher))]
    [ComponentData(Name)]
    public sealed class FileCommandDispatcher : CommandDispatcher
    {
        public const string Name = "file";
        private SwitchBufferControl switchBufferControl;

        [Import]
        private IBufferManager bufferManager = null;

        [Command]
        public void SwitchBuffer()
        {
            var buffers = bufferManager.EnumerateBuffers()
                .OrderByDescending(b => b.LastAccess)
                .ToList();

            if (buffers.Count < 2)
                return;

            var cur = ViewManager.GetActiveView()?.Buffer;
            var idx = cur != null ? buffers.IndexOf(cur) : 0;

            if (switchBufferControl == null)
            {
                switchBufferControl = new SwitchBufferControl();
                switchBufferControl.CloseRequested += (o, ev) =>
                {
                    switchBufferControl.FindForm().Controls.Remove(switchBufferControl);
                    var newBuf = switchBufferControl.Buffers[switchBufferControl.SelectedIndex];
                    var view = ViewManager.EnumerateViews()
                        .FirstOrDefault(v => v.Buffer == newBuf);

                    if (view != null)
                        ViewManager.ActivateView(view);
                    else
                        OpenBuffer(newBuf);
                };
            }

            var oldFrm = switchBufferControl.FindForm();

            if (oldFrm != null)
                oldFrm.Controls.Remove(switchBufferControl);

            var frm = Form.ActiveForm;
            switchBufferControl.Buffers = buffers;
            switchBufferControl.Width = frm.Width / 3;
            switchBufferControl.Height = switchBufferControl.CalculateHeight();
            switchBufferControl.Left = (frm.Width - switchBufferControl.Width) / 2;
            switchBufferControl.Top = (frm.Height - switchBufferControl.Height) / 2;
            switchBufferControl.SelectedIndex = idx + 1 >= buffers.Count ? 0 : idx + 1;
            frm.Controls.Add(switchBufferControl);
            switchBufferControl.BringToFront();
            switchBufferControl.Focus();
        }

        [Command]
        public void OpenFile(string fileName, Encoding enc = null)
        {
            var fi = default(FileInfo);

            if (!FileUtil.TryGetInfo(fileName, out fi))
                return;

            if (enc == null && fi.Exists && !FileUtil.HasBom(fi))
                enc = UTF8EncodingNoBom.Instance;
            else
                enc = Encoding.UTF8;

            var buffer = bufferManager.CreateBuffer(fi, enc);
            OpenBuffer(buffer);
        }

        [Command]
        public void CloseFile()
        {
            var buffer = GetActiveBuffer();

            if (buffer.IsDirty)
            {
                var sb = new StringBuilder();
                var res = MessageBox.Show(Application.OpenForms[0],
                    $"Do you want to save the changes made to {buffer.File.Name}?",
                    Application.ProductName,
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Warning);

                if (res == DialogResult.Yes)
                    SaveFile();
                else if (res == DialogResult.Cancel)
                    return;
            }

            ViewManager.GetActiveView().DetachBuffer();
            bufferManager.CloseBuffer(buffer);

            var next = bufferManager.EnumerateBuffers()
                .OrderByDescending(b => b.LastAccess)
                .FirstOrDefault();

            if (next == null)
                next = bufferManager.CreateBuffer();

            OpenBuffer(next);
        }

        [Command]
        public void ReopenFile(Encoding enc)
        {
            var buffer = ViewManager.GetActiveView()?.Buffer;

            if (buffer != null)
            {
                if (buffer.IsDirty)
                    App.Ext.Log("File is dirty. Save file before reloading.", EntryType.Error);
                else if (!buffer.File.Exists)
                    buffer.Encoding = enc;
                else
                {
                    bufferManager.CloseBuffer(buffer);
                    OpenFile(buffer.File.FullName, enc);
                }
            }
        }

        private bool OpenFolder(DirectoryInfo dir)
        {
            return App.Catalog<IWorkspaceController>().Default().OpenWorkspace(dir);
        }

        [Command]
        public void NewFile()
        {
            var buffer = bufferManager.CreateBuffer();
            OpenBuffer(buffer);
        }

        [Command]
        public void RevertFile()
        {
            var buf = ViewManager.GetActiveView()?.Buffer;

            if (buf != null && buf.File.Exists)
            {
                bufferManager.CloseBuffer(buf);
                OpenFile(buf.File.FullName);
            }
            else if (buf != null && !buf.File.Exists)
                App.Ext.Log($"Unable to revert a file {buf.File}. File is not yet written to disk.", EntryType.Error);
        }

        [Command]
        public void SaveFile(string fileName = null)
        {
            var buffer = GetActiveBuffer();

            if (buffer == null)
                return;

            var fi = default(FileInfo);

            if (fileName != null)
            {
                if (!FileUtil.TryGetInfo(fileName, buffer, out fi))
                    return;
            }
            else
                fi = buffer.File;

            OpenFolder(fi.Directory);
            bufferManager.SaveBuffer(buffer, fi, buffer.Encoding);
        }

        [Command]
        public void SaveCopy(string fileName)
        {
            var buffer = GetActiveBuffer();

            if (buffer == null)
                return;

            FileUtil.WriteFile(fileName, buffer.GetContents(), buffer.Encoding);
        }

        [Command]
        public void RenameFile(string name)
        {
            if (name.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
            {
                App.Ext.Log($"File name '{name}' is invalid.", EntryType.Error);
                return;
            }

            var buffer = GetActiveBuffer();

            if (buffer == null)
                return;

            var fi = default(FileInfo);

            if (!FileUtil.TryGetInfo(name, buffer, out fi))
                return;

            if (fi.Exists)
            {
                App.Ext.Log($"File '{fi.FullName}' already exists.", EntryType.Error);
                return;
            }

            var old = buffer.File;
            bufferManager.SaveBuffer(buffer, fi, buffer.Encoding);
            FileUtil.TryDelete(old);
        }

        [Command]
        public void CopyFilePath()
        {
            var buffer = GetActiveBuffer();

            if (buffer == null)
                return;

            Clipboard.SetText(buffer.File.FullName, TextDataFormat.UnicodeText);
            App.Ext.Log($"File path {buffer.File.FullName} is copied to clipboard.", EntryType.Info);
        }

        private IBuffer GetActiveBuffer() => ViewManager.GetActiveView()?.Buffer;

        [Command]
        public void OpenRecentFile(string fileName)
        {
            var buf = bufferManager.EnumerateBuffers()
                .FirstOrDefault(b => b.File.Name.IndexOf(fileName, StringComparison.OrdinalIgnoreCase) != -1);
            OpenBuffer(buf);
        }

        [Command]
        public void OpenModifiedFile(string fileName) => OpenRecentFile(fileName);

        private void OpenBuffer(IBuffer buf)
        {
            if (buf != null)
            {
                var view = ViewManager.GetActiveView();

                if (buf.File.Directory != null && OpenFolder(buf.File.Directory))
                    App.Ext.Log($"Workspace opened: {view.Workspace}", EntryType.Info);

                view.AttachBuffer(buf);
                App.Ext.Log($"Buffer opened: {buf.File}", EntryType.Info);
            }
        }
    }
}
