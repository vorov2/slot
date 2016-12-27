using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.IO;
using Slot.Core.ComponentModel;
using Slot.Core.ViewModel;
using Slot.Editor.ObjectModel;
using Slot.Core;
using Slot.Core.Output;
using Slot.Core.State;
using System.Windows.Forms;
using Slot.Core.CommandModel;

namespace Slot.Editor
{
    [Export(typeof(IBufferManager))]
    [ComponentData(Name)]
    public sealed class DocumentBufferManager : IBufferManager
    {
        public const string Name = "buffermanager.default";
        private const int MAX_RECENT = 100;
        private const int BIN_VERSION = 1;

        private readonly List<IBuffer> buffers = new List<IBuffer>();
        private readonly List<Recent> recents = new List<Recent>();
        private volatile bool recentRead = false;
        private readonly static Guid stateId = Guid.Parse("CFFFB195-B2FF-40F6-9908-D6D403C965EC");

        [Import]
        private IStateManager stateManager = null;

        class Recent
        {
            public FileInfo File;
            public DateTime Date;
        }

        public DocumentBufferManager()
        {
            App.Exit += AppExit;
        }

        private bool CanExit(ExitEventArgs e)
        {
            var seq = EnumerateBuffers().Where(b => b.IsDirty);

            if (seq.Any())
            {
                var sb = new StringBuilder();
                var res = MessageBox.Show(Form.ActiveForm,
                    $"Do you want to save the changes to the following files?\n\n{string.Join("\n", seq.Select(f => f.File.Name))}",
                    Application.ProductName,
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Warning);

                if (res == DialogResult.Yes)
                {
                    var cmd = (Identifier)"file.saveFile";
                    var exec = App.Catalog<ICommandDispatcher>().GetComponent(cmd.Namespace);

                    foreach (var d in seq)
                        exec.Execute(null, cmd, d.File.FullName);
                }

                if (res == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return false;
                }
            }

            return true;
        }

        private void AppExit(object sender, ExitEventArgs e)
        {
            if (!CanExit(e))
                return;

            var stream = stateManager.WriteState(stateId);

            if (stream != null)
                using (var bw = new BinaryWriter(stream))
                {
                    bw.Write(BIN_VERSION);
                    bw.Write(recents.Count);

                    foreach (var r in recents)
                    {
                        bw.Write(r.Date.Ticks);
                        bw.Write(r.File.FullName);
                    }

                    bw.Write(buffers.Count);

                    foreach (var b in buffers)
                    {
                        bw.Write(b.Id.ToByteArray());
                        bw.Write(b.LastAccess.Ticks);
                        bw.Write(b.Encoding is UTF8EncodingNoBom ? -b.Encoding.CodePage : b.Encoding.CodePage);
                        bw.Write(b.File.FullName);
                    }

                    App.Ext.Log($"State saved for {recents.Count} recent item(s).", EntryType.Info);
                }

            var count = 0;

            foreach (var b in buffers)
            {
                stream = stateManager.WriteState(b.Id);

                if (stream != null)
                {
                    count++;
                    using (stream)
                        b.SerializeState(stream);
                }
            }

            App.Ext.Log($"State saved for {count} buffer(s).", EntryType.Info);
        }

        private void ReadState()
        {
            if (recentRead)
                return;

            var stream = stateManager.ReadState(stateId);

            if (stream != null)
            {
                using (var br = new BinaryReader(stream))
                {
                    var v = br.ReadInt32();

                    if (v != BIN_VERSION)
                    {
                        App.Ext.Log($"Invalid buffer manager state version. Expected {BIN_VERSION}, got {v}.", EntryType.Error);
                        recentRead = true;
                        return;
                    }

                    var count = br.ReadInt32();

                    for (var i = 0; i < count; i++)
                        recents.Add(new Recent {
                            Date = new DateTime(br.ReadInt64()),
                            File = new FileInfo(br.ReadString())
                        });

                    App.Ext.Log($"State restored for {count} recent item(s).", EntryType.Info);
                    count = br.ReadInt32();

                    for (var i = 0; i < count; i++)
                    {
                        var id = new Guid(br.ReadBytes(16));
                        var dt = new DateTime(br.ReadInt64());
                        var cp = br.ReadInt32();
                        var fi = new FileInfo(br.ReadString());
                        var enc = cp < 0 ? UTF8EncodingNoBom.Instance : Encoding.GetEncoding(cp);

                        if (fi.Exists)
                        {
                            var buf = InternalCreateBuffer(fi, enc, id);
                            buf.LastAccess = dt;
                        }
                    }

                    foreach (var b in buffers)
                    {
                        stream = stateManager.ReadState(b.Id);

                        if (stream != null)
                            using (stream)
                                b.DeserializeState(stream);
                    }

                    App.Ext.Log($"State restored for {count} buffer(s).", EntryType.Info);
                }
            }

            recentRead = true;
        }

        public IBuffer CreateBuffer()
        {
            ReadState();
            var num = buffers.Count(b => !b.File.Exists);
            return InternalCreateBuffer(new FileInfo($"untitled-{num + 1}"), Encoding.UTF8, Guid.NewGuid());
        }

        public void CloseBuffer(IBuffer buffer)
        {
            var idx = buffers.IndexOf(buffer);

            if (idx != -1 && !buffer.Bound)
                buffers.RemoveAt(idx);

            stateManager.ClearState(buffer.Id);
        }

        public void SaveBuffer(IBuffer buffer, FileInfo file, Encoding encoding)
        {
            var docb = buffer as DocumentBuffer;

            if (docb == null)
            {
                App.Ext.Log("Invalid document type.", EntryType.Error);
                return;
            }

            var txt = docb.GetText();

            if (!FileUtil.WriteFile(file, txt, encoding))
                return;

            docb.File = file;
            docb.Encoding = encoding;
            docb.ClearDirtyFlag();
        }

        public IBuffer CreateBuffer(FileInfo fileName, Encoding encoding)
        {
            ReadState();
            fileName.Refresh();

            if (!fileName.Exists)
            {
                var buf = InternalCreateBuffer(fileName, encoding, Guid.NewGuid());
                buf.Edits++;
                return buf;
            }

            return InternalCreateBuffer(fileName, encoding, Guid.NewGuid());
        }

        private Document CreateDocument(FileInfo fileName, Encoding encoding)
        {
            fileName.Refresh();

            if (!fileName.Exists)
                return Document.FromString("");

            string txt = null;
            var res = FileUtil.ReadFile(fileName, encoding, out txt);
            return res ? Document.FromString(txt) : null;
        }

        private DocumentBuffer InternalCreateBuffer(FileInfo file, Encoding enc, Guid id)
        {
            var buf = buffers.FirstOrDefault(b => 
                b.File.FullName.Equals(file.FullName, StringComparison.OrdinalIgnoreCase));

            if (buf == null)
            {
                var doc = CreateDocument(file, enc);

                if (doc == null)
                    return null;

                buf = new DocumentBuffer(doc, file, enc, id);
                buffers.Add(buf);
                var rd = recents.FirstOrDefault(r => r.File.FullName.Equals(file.FullName, StringComparison.OrdinalIgnoreCase));

                if (rd != null)
                    rd.Date = DateTime.Now;
                else
                    recents.Add(new Recent { File = file, Date = DateTime.Now });

                if (recents.Count > MAX_RECENT)
                    recents.RemoveAt(0);
            }

            return (DocumentBuffer)buf;
        }

        public IEnumerable<IBuffer> EnumerateBuffers()
        {
            ReadState();
            return buffers.OrderByDescending(b => b.LastAccess);
        }

        public IEnumerable<FileInfo> EnumerateRecent()
        {
            ReadState();
            return recents.OrderByDescending(r => r.Date).Select(r => r.File);
        }
    }
}
