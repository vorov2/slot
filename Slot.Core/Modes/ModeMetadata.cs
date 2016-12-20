using System;
using System.Collections.Generic;
using System.IO;

namespace Slot.Core.Modes
{
    public sealed class ModeMetadata
    {
        internal ModeMetadata(Identifier key, string name, ModeKind kind, IEnumerable<string> extensions)
        {
            Key = key;
            Name = name;
            Kind = kind;
            Extensions = extensions;
        }

        public Identifier Key { get; }

        public string Name { get; }

        public ModeKind Kind { get; }

        public IEnumerable<string> Extensions { get; }

        public bool Match(FileInfo file)
        {
            var ext = file.Extension.TrimStart('.');

            foreach (var e in Extensions)
                if (string.Equals(e, ext, StringComparison.OrdinalIgnoreCase))
                    return true;

            return false;
        }

        public override string ToString() => $"{Key} ({Name})";
    }
}
