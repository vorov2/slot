using CodeBox.Core.ComponentModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CodeBox.Core.ViewModel
{
    public interface IBufferManager : IComponent
    {
        IMaterialBuffer CreateBuffer();

        IMaterialBuffer CreateBuffer(FileInfo fileName, Encoding encoding);

        void SaveBuffer(IMaterialBuffer buffer, FileInfo file, Encoding encoding);

        IEnumerable<IBuffer> EnumerateBuffers();
    }
}
