using Slot.Core.ComponentModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Slot.Core.ViewModel
{
    public interface IBufferManager : IComponent
    {
        IBuffer CreateBuffer();

        IBuffer CreateBuffer(FileInfo fileName, Encoding encoding);

        void CloseBuffer(IBuffer buf);

        void SaveBuffer(IBuffer buffer, FileInfo file, Encoding encoding);

        IEnumerable<IBuffer> EnumerateBuffers();

        IEnumerable<FileInfo> EnumerateRecent();
    }
}
