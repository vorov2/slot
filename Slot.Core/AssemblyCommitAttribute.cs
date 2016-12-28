using System;

namespace Slot.Core
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class AssemblyCommitAttribute : Attribute
    {
        public AssemblyCommitAttribute(string hash)
        {
            Hash = hash;
        }

        public string Hash { get; }

        public override string ToString() => Hash;
    }
}
