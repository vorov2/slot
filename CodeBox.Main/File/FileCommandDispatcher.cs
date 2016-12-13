using CodeBox.Core;
using CodeBox.Core.CommandModel;
using CodeBox.Core.ComponentModel;
using CodeBox.Core.Output;
using CodeBox.Core.ViewModel;
using CodeBox.Core.Workspaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeBox.Main.File
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
        public void SwitchView(string viewName)
        {
            var view = viewManager.EnumerateViews()
                 .FirstOrDefault(v => v.Buffer.File.Name.Contains(viewName));
            viewManager.ActivateView(view);
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
                    var newBuf = switchBufferControl.Buffers[switchBufferControl.SelectedIndex];
                    var view = viewManager.EnumerateViews()
                        .FirstOrDefault(v => v.Buffer == newBuf);

                    if (view != null)
                        viewManager.ActivateView(view);
                    else
                        OpenBuffer(newBuf);

                    switchBufferControl.FindForm().Controls.Remove(switchBufferControl);
                };
            }

            var oldFrm = switchBufferControl.FindForm();

            if (oldFrm != null)
                oldFrm.Controls.Remove(switchBufferControl);

            var frm = Form.ActiveForm;
            switchBufferControl.Buffers = buffers;
            switchBufferControl.Width = frm.Width / 2;
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
            {
                App.Ext.Log($"Invalid file name: {fileName}.", EntryType.Error);
                return;
            }

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
                    OpenFile(buffer.File.FullName, enc);
            }
        }

        [Command]
        public void OpenFolder(string dir)
        {
            App.Catalog<IWorkspaceController>().First().CreateWorkspace(new DirectoryInfo(dir));
        }

        [Command]
        public void CloseFolder()
        {
            App.Catalog<IWorkspaceController>().First().CloseWorkspace();
        }

        private bool TryOpenFolder(string dir)
        {
            return App.Catalog<IWorkspaceController>().First().OpenWorkspace(new DirectoryInfo(dir));
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

            var fi = fileName != null
                ? new FileInfo(Path.Combine(buffer.File?.Directory != null
                    ? buffer.File.Directory.FullName : Environment.CurrentDirectory, fileName))
                : buffer.File;
            bufferManager.SaveBuffer(buffer, fi, buffer.Encoding);
            TryOpenFolder(fi.DirectoryName);
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

        [Command]
        public void CloseWindow()
        {
            var view = viewManager.GetActiveView();
            view.Close();
        }

        private void OpenBuffer(IBuffer buf)
        {
            if (buf != null)
            {
                var view = viewManager.GetActiveView();
                view.AttachBuffer(buf);

                if (buf.File.Directory != null && !TryOpenFolder(buf.File.DirectoryName))
                    Directory.SetCurrentDirectory(buf.File.Directory.FullName);
            }
        }
    }
}
