using System;
using System.IO;
using System.Text;

namespace CodeBox.Core.ViewModel
{
    public interface IBuffer
    {
        FileInfo File { get; }

        Encoding Encoding { get; }

        DateTime LastAccess { get; }
    }
}
