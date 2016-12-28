using System;

namespace Slot.Core.CommandModel
{
    public sealed class ArgumentMetadata : IEquatable<ArgumentMetadata>
    {
        public string Name { get; internal set; }

        public Identifier ValueProvider { get; internal set; }

        public bool Optional { get; internal set; }

        public ArgumentAffinity Affinity { get; set; }

        public override string ToString() => Name + (Optional ? "?" : "");

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (Name ?? "").GetHashCode();
                if (ValueProvider != null)
                    hash = hash * 23 + ValueProvider.GetHashCode();
                hash = hash * 23 + Optional.GetHashCode();
                hash = hash * 23 + Affinity.GetHashCode();
                return hash;
            }
        }

        public bool Equals(ArgumentMetadata other) => other != null && other.Name == Name
            && other.ValueProvider == ValueProvider && other.Optional == Optional
            && other.Affinity == Affinity;

        public override bool Equals(object obj) => Equals(obj as ArgumentMetadata);
    }
}
