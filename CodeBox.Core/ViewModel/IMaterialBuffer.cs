using System;
using System.IO;

namespace CodeBox.Core.ViewModel
{
    public interface IMaterialBuffer : IBuffer
    {
        string Mode { get; set; }

        bool IsDirty { get; }

        void Serialize(Stream stream);

        void Truncate(string text = "");

        string GetContents();
    }
}
