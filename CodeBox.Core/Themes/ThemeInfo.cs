using System;
using System.IO;

namespace CodeBox.Core.Themes
{
    public sealed class ThemeInfo
    {
        public ThemeInfo(Identifier key, string name, FileInfo file)
        {
            Key = key;
            Name = name;
            File = file;
        }

        public Identifier Key { get; }

        public string Name { get; }

        public FileInfo File { get; }
    }
}
