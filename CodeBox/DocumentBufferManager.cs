using CodeBox.Core.ComponentModel;
using CodeBox.Core.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CodeBox.ObjectModel;
using System.Security;
using CodeBox.Core;

namespace CodeBox
{
    [Export(typeof(IBufferManager))]
    [ComponentData(Name)]
    public sealed class DocumentBufferManager : IBufferManager
    {
        public const string Name = "buffermanager.default";

        private readonly Stack<IBuffer> buffers = new Stack<IBuffer>();

        public IMaterialBuffer CreateBuffer()
        {
            var num = buffers.Count(b => !b.File.Exists);
            var ret = InternalCreateBuffer(Document.FromString(""),
                new FileInfo($"untitled-{num + 1}"), Encoding.UTF8);
            ret.Edits++;
            return ret;
        }

        public void SaveBuffer(IMaterialBuffer buffer, FileInfo file, Encoding encoding)
        {
            var docb = buffer as DocumentBuffer;

            if (docb == null)
            {
                //Log
                return;
            }

            file.Refresh();

            if (!file.Directory.Exists)
            {
                var res = file.Directory.Exec(d => d.Create());

                if (!res.Success)
                {
                    //Log
                    return;
                }
            }

            var txt = docb.GetText();
            var res1 = file.Exec(fl => File.WriteAllText(fl.FullName, txt, encoding));

            if (res1.Success)
            {
                docb.File = file;
                docb.Encoding = encoding;
                docb.ClearDirtyFlag();
            }
            else
            {
                Console.WriteLine(res1.Reason);
                //Log
            }
        }

        public IMaterialBuffer CreateBuffer(FileInfo fileName, Encoding encoding)
        {
            fileName.Refresh();

            if (!fileName.Exists)
                return CreateBuffer();

            string txt = null;
            var res = fileName.Exec(fl => txt = File.ReadAllText(fl.FullName, encoding));

            if (res.Success)
                return InternalCreateBuffer(Document.FromString(txt), fileName, encoding);
            else
            {
                //Log
                return null;
            }
        }

        private DocumentBuffer InternalCreateBuffer(Document doc, FileInfo file, Encoding enc)
        {
            var buf = new DocumentBuffer(doc, file, enc);
            buffers.Push(buf);
            return buf;
        }

        public IEnumerable<IBuffer> EnumerateBuffers()
        {
            return buffers;
        }
    }
}
