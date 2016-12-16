using System;
using System.IO;

namespace Slot.Core.ViewModel
{
    public interface IMaterialBuffer : IBuffer
    {
        bool IsDirty { get; }

        void SerializeState(Stream stream);

        void DeserializeState(Stream stream);

        void Truncate(string text = "");

        string GetContents();
    }
}
