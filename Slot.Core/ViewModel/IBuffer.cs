using System;
using System.IO;
using System.Text;

namespace Slot.Core.ViewModel
{
    public interface IBuffer
    {
        void SerializeState(Stream stream);

        void DeserializeState(Stream stream);

        void Truncate(string text = "");

        string GetContents();

        void ClearDirtyFlag();

        Guid Id { get; }

        bool IsDirty { get; }

        FileInfo File { get; set; }

        Encoding Encoding { get; set; }

        DateTime LastAccess { get; set; }
    }
}
