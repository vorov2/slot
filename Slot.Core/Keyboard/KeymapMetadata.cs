using System;
using System.IO;

namespace Slot.Core.Keyboard
{
    internal sealed class KeymapMetadata
    {
        public Identifier Key { get; set; }

        public string Name { get; set; }

        public FileInfo File { get; set; }
    }
}
