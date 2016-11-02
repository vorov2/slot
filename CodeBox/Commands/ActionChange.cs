using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Commands
{
    public enum ActionChange
    {
        None,

        Mixed,

        Atomic,

        Forward,

        Backward
    }
}
