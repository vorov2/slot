using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.ObjectModel
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

    public enum ActionExponent
    {
        None = 0x00,

        Silent = 0x01,

        RestoreCaret = 0x02,

        Scroll = 0x04,

        SingleCursor = 0x08,

        ClearSelections = 0x10,

        Undoable = 0x20
    }
}
