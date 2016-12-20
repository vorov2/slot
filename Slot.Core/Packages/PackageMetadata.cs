using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Json;

namespace Slot.Core.Packages
{
    public sealed class PackageMetadata
    {
        internal PackageMetadata(Identifier key, string name, string version, DirectoryInfo dir, Dictionary<string, object> metadata)
        {
            Key = key;
            Name = name;
            Version = version;
            Directory = dir;
            Metadata = metadata;
        }

        public Identifier Key { get; }

        public string Name { get; }

        public string Version { get; }

        public DirectoryInfo Directory { get; }

        internal Dictionary<string, object> Metadata { get; }

        public IEnumerable<Dictionary<string, object>> GetMetadata(PackageSection section)
        {
            var obj = Metadata.Object(section.ToString()) as List<object>;
            return obj != null ? obj.OfType<Dictionary<string, object>>()
                : Enumerable.Empty<Dictionary<string, object>>();
        }

        public override string ToString() => $"{Key} ({Name})";
    }
}
