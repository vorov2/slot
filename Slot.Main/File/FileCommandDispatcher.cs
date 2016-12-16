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
        private IViewManager viewManager = null;

        [Import]
        private IBufferManager bufferManager = null;

        [Command]
        public void NewWindow()
        {
            var act = viewManager.GetActiveView();
            var view = viewManager.CreateView();
            view.AttachBuffer(act.Buffer);
        }

        [Command]
        public void SwitchWindow(string viewName)
        {
            var view = viewManager.EnumerateViews()
                 .FirstOrDefault(v => v.Buffer.File.Name.Contains(viewName));
            viewManager.ActivateView(view);
        }

        [Command]
        public void CloseWindow()
        {
            var view = viewManager.GetActiveView();
            view.Close();
        }

        [Command]
        public void SwitchBuffer()
        {
            var buffers = bufferManager.EnumerateBuffers()
                .OrderByDescending(b => b.LastAccess)
                .ToList();

            if (buffers.Count < 2)
                return;

            var cur = viewManager.GetActiveView()?.Buffer;
            var idx = cur != null ? buffers.IndexOf(cur) : 0;

            if (switchBufferControl == null)
            {
                switchBufferControl = new SwitchBufferControl();
                switchBufferControl.CloseRequested += (o, ev) =>
                {
                    switchBufferControl.FindForm().Controls.Remove(switchBufferControl);
                    var newBuf = switchBufferControl.Buffers[switchBufferControl.SelectedIndex];
                    var view = viewManager.EnumerateViews()
                        .FirstOrDefault(v => v.Buffer == newBuf);

                    if (view != null)
                        viewManager.ActivateView(view);
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

            var buffer = bufferManager.CreateBuffer(fi, enc ?? Encoding.UTF8);
            OpenBuffer(buffer);
        }

        [Command]
        public void ReopenFile(Encoding enc)
        {
            var buffer = viewManager.GetActiveView()?.Buffer as IMaterialBuffer;

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

        [Command]
        public void OpenFolder(string dir)
        {
            var dirInfo = default(DirectoryInfo);

            if (!FileUtil.TryGetInfo(dir, out dirInfo))
                return;

            App.Catalog<IWorkspaceController>().Default().OpenWorkspace(dirInfo);
        }

        private void OpenFolder(DirectoryInfo dir)
        {
            App.Catalog<IWorkspaceController>().Default().OpenWorkspace(dir);
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
            var buf = viewManager.GetActiveView()?.Buffer;

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
        }

        private IMaterialBuffer GetActiveBuffer()
        {
            var view = viewManager.GetActiveView();
            var buffer = view.Buffer as IMaterialBuffer;

            if (buffer == null)
            {
                //Log
                //basically impossible situation
                return null;
            }

            return buffer;
        }

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
                if (buf.File.Directory != null)
                    OpenFolder(buf.File.Directory);

                var view = viewManager.GetActiveView();
                view.AttachBuffer(buf);
            }
        }
    }
}
