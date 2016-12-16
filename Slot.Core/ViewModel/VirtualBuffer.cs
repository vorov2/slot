using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slot.Core.ViewModel
{
    public sealed class VirtualBuffer : IBuffer
    {
        public VirtualBuffer(FileInfo file) : this(file, Encoding.UTF8)
        {

        }

        public VirtualBuffer(FileInfo file, Encoding encoding)
        {
            File = file;
            Encoding = encoding;
        }

        public Encoding Encoding { get; set; }

        public FileInfo File { get; }

        public DateTime LastAccess => DateTime.MinValue;
    }
}
