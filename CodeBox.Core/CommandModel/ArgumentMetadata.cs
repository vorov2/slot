using System;

namespace CodeBox.Core.CommandModel
{
    public sealed class ArgumentMetadata
    {
        public string Name { get; internal set; }

        public Identifier ValueProvider { get; internal set; }

        public bool Optional { get; internal set; }

        public ArgumentAffinity Affinity { get; set; }

        public override string ToString()
        {
            return Name + (Optional ? "?" : "");
        }
    }
}
