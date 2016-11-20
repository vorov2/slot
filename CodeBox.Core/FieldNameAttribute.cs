using System;

namespace CodeBox.Core
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class FieldNameAttribute : Attribute
    {
        public FieldNameAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public override string ToString() => Name;
    }
}
