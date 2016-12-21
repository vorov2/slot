using System;
using System.IO;

namespace Slot.Core.Keyboard
{
    public sealed class KeymapMetadata
    {
        public KeymapMetadata(Identifier key, string name, FileInfo file)
        {
            Key = key;
            Name = name;
            File = file;
        }

        public Identifier Key { get; set; }

        public string Name { get; set; }

        public FileInfo File { get; set; }
    }
}
