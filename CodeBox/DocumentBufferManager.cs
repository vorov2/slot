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

namespace CodeBox
{
    [Export(Name, typeof(IComponent))]
    [ComponentData(Name)]
    public sealed class DocumentBufferManager : IBufferManager
    {
        public const string Name = "buffermanager.default";
        private readonly Stack<IBuffer> buffers = new Stack<IBuffer>();

        public IBuffer CreateBuffer()
        {
            return InternalCreateBuffer(Document.FromString(""), new FileInfo("untitled"), Encoding.UTF8);
        }

        public IBuffer CreateBuffer(FileInfo fileName, Encoding encoding)
        {
            fileName.Refresh();

            if (!fileName.Exists)
                return CreateBuffer();

            var txt = File.ReadAllText(fileName.FullName, encoding);
            return InternalCreateBuffer(Document.FromString(txt), fileName, encoding);
        }

        private IBuffer InternalCreateBuffer(Document doc, FileInfo file, Encoding enc)
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
