using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Commands
{
    public enum ActionResult
    {
        None,

        Standard,

        Mixed,

        Atomic,

        Forward,

        Backward
    }
}
