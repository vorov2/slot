using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Commands
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class CommandBehaviorAttribute : Attribute
    {
        public CommandBehaviorAttribute(ActionExponent exponent)
        {
            Exponent = exponent;
        }

        public ActionExponent Exponent { get; private set; }
    }
}
