using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
