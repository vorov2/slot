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
using Slot.Core.Messages;

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
            var buffers = bufferManager.EnumerateBuffers().ToList();

            if (buffers.Count < 2)
                return;

            var cur = ViewManager.ActiveView?.Buffer;
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

            OpenFile(fi, enc);
        }

        private void OpenFile(FileInfo fi, Encoding enc = null)
        {
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
                var res = App.Ext.Show(
                    $"Do you want to save the changes made to {buffer.File.Name}?",
                    "Your changes will be lost if you don't save them.",
                    MessageButtons.Save | MessageButtons.DontSave | MessageButtons.Cancel);

                if (res == MessageButtons.Save)
                    SaveFile();
                else if (res == MessageButtons.Cancel)
                    return;
            }

            ViewManager.ActiveView.DetachBuffer();
            bufferManager.CloseBuffer(buffer);
            var next = bufferManager.EnumerateBuffers().FirstOrDefault();

            if (next == null)
                next = bufferManager.CreateBuffer();

            OpenBuffer(next);
        }

        [Command]
        public void ReopenFile(Encoding enc)
        {
            var buffer = ViewManager.ActiveView?.Buffer;

            if (buffer != null)
            {
                if (!buffer.File.Exists)
                    buffer.Encoding = enc;
                else if (buffer.IsDirty)
                    App.Ext.Log("File is dirty. Save file before reloading.", EntryType.Error);
                else
                {
                    bufferManager.CloseBuffer(buffer);
                    OpenFile(buffer.File.FullName, enc);
                }
            }
        }

        private static bool OpenFolder(IView view, DirectoryInfo dir)
        {
            return App.Component<IWorkspaceController>().OpenWorkspace(view, dir);
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
            var buf = ViewManager.ActiveView?.Buffer;

            if (buf != null && buf.File.Exists)
            {
                bufferManager.CloseBuffer(buf);
                OpenFile(buf.File.FullName);
            }
            else if (buf != null && !buf.File.Exists)
                App.Ext.Log($"Unable to revert a file {buf.File}. File is not yet written to disk.", EntryType.Error);
        }

        [Command]
        public void SaveFile(string fileName = null, Encoding enc = null)
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

            OpenFolder(ViewManager.ActiveView, fi.Directory);
            bufferManager.SaveBuffer(buffer, fi, enc ?? buffer.Encoding);
        }

        [Command]
        public void SaveWithEncoding(Encoding enc)
        {
            var buffer = GetActiveBuffer();

            if (buffer == null)
                return;

            if (!FileUtil.EnsureFilePath(buffer.File))
                return;

            bufferManager.SaveBuffer(buffer, buffer.File, enc);
        }

        [Command]
        public void SaveCopy(string fileName = null)
        {
            var buffer = GetActiveBuffer();

            if (buffer == null)
                return;

            fileName = fileName ?? FileUtil.GenerateFileName(
                buffer.File.FullName.Replace(buffer.File.Extension, "{0}" + buffer.File.Extension));
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

        private IBuffer GetActiveBuffer() => ViewManager.ActiveView?.Buffer;

        [Command]
        public void OpenRecentFile(string fileName, Encoding enc = null)
        {
            var file = bufferManager.EnumerateRecent()
                .FirstOrDefault(b => b.FullName.IndexOf(fileName, StringComparison.OrdinalIgnoreCase) != -1);
            OpenFile(file, enc);
        }

        [Command]
        public void OpenModifiedFile(string fileName)
        {
            var buf = bufferManager.EnumerateBuffers()
                .FirstOrDefault(b => b.File.FullName.IndexOf(fileName, StringComparison.OrdinalIgnoreCase) != -1);
            OpenBuffer(buf);
        }

        internal static void OpenBuffer(IBuffer buf, IView view = null)
        {
            if (buf != null)
            {
                view = view ?? App.Component<IViewManager>().ActiveView;

                if (buf.File.Directory != null && OpenFolder(view, buf.File.Directory))
                    App.Ext.Log($"Workspace opened: {view.Workspace}", EntryType.Info);

                view.AttachBuffer(buf);
                App.Ext.Log($"Buffer opened: {buf.File}", EntryType.Info);
            }
        }
    }
}
