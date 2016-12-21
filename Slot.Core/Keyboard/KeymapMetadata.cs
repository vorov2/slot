using System;
using System.IO;

namespace Slot.Core.Keyboard
{
    public sealed class KeymapMetadata
    {
        public Identifier Key { get; internal set; }

        public string Name { get; internal set; }

        public FileInfo File { get; internal set; }
    }
}
