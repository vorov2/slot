using System;
using System.IO;
using System.Text;

namespace Slot.Core.ViewModel
{
    public interface IBuffer
    {
        FileInfo File { get; }

        Encoding Encoding { get; set; }

        DateTime LastAccess { get; }
    }
}
