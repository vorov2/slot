using CodeBox.Core;
using CodeBox.Core.CommandModel;
using CodeBox.Core.ComponentModel;
using CodeBox.Core.ViewModel;
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
    [Export(typeof(IComponent))]
    [ComponentData(Key)]
    public sealed class FileCommandDispatcher : CommandDispatcher
    {
        public const string Key = "file";

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
                .OfType<IMaterialBuffer>()
                .ToList();

            if (buffers.Count < 2)
                return;

            var cur = viewManager.GetActiveView()?.Buffer as IMaterialBuffer;
            var idx = cur != null ? buffers.IndexOf(cur) : 0;

            if (idx < buffers.Count - 1)
                idx++;
            else
                idx = 0;

            var newBuf = buffers[idx];
            var view = viewManager.EnumerateViews()
                .FirstOrDefault(v => v.Buffer == newBuf);

            if (view != null)
                viewManager.ActivateView(view);
            else
                OpenBuffer(newBuf);
        }

        [Command]
        public void OpenFile(string fileName, Encoding enc = null)
        {
            var fi = new FileInfo(Uri.UnescapeDataString(fileName));
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
                {
                    //Log
                    return;
                }
                else if (!buffer.File.Exists)
                {
                    //Log
                    return;
                }

                OpenFile(buffer.File.FullName, enc);
            }
        }

        [Command]
        public void NewFile()
        {
            var buffer = bufferManager.CreateBuffer();
            OpenBuffer(buffer);
        }

        [Command]
        public void SaveFile(string fileName = null)
        {
            var view = viewManager.GetActiveView();
            var buffer = view.Buffer as IMaterialBuffer;

            if (buffer == null)
            {
                //basically impossible situation
                return;
            }

            var fi = fileName != null
                ? new FileInfo(Path.Combine(buffer.File?.Directory != null
                    ? buffer.File.Directory.FullName : Environment.CurrentDirectory, fileName))
                : buffer.File;
            bufferManager.SaveBuffer(buffer, fi, buffer.Encoding);
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

                if (buf?.File.Directory != null)
                    Directory.SetCurrentDirectory(buf.File.Directory.FullName);
            }
        }
    }
}
