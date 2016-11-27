using System;
using System.IO;

namespace CodeBox.Core.ViewModel
{
    public interface IMaterialBuffer : IBuffer
    {
        bool IsDirty { get; }

        void Serialize(Stream stream);
    }
}
