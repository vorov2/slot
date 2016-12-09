using System;

namespace CodeBox.Core.Settings
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class SettingAttribute : Attribute
    {
        public SettingAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public override string ToString() => Name;
    }
}
