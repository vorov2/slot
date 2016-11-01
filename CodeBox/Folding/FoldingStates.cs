using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Folding
{
    [Flags]
    public enum FoldingStates : byte
    {
        None = 0x00,

        Invisible = 0x01,

        Header = 0x02
    }

    public static class VisibleStatesExtensions
    {
        public static bool Has(this FoldingStates enu, FoldingStates flag)
        {
            return (enu & flag) == flag;
        }
    }
}
