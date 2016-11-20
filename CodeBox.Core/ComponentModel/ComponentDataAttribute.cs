using System;
using System.ComponentModel.Composition;

namespace CodeBox.Core.ComponentModel
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ComponentDataAttribute : Attribute, IComponentMetadata
    {
        public ComponentDataAttribute(string key)
        {
            Key = key;
        }

        public string Key { get; }
    }
}
