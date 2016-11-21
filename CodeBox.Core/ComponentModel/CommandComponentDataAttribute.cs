using System;
using System.ComponentModel.Composition;

namespace CodeBox.Core.ComponentModel
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class CommandComponentDataAttribute : Attribute, ICommandComponentMetadata
    {
        public CommandComponentDataAttribute(string key, string alias)
        {
            Key = key;
            Alias = alias;
        }

        public string Key { get; }

        public string Alias { get; }
    }
}
