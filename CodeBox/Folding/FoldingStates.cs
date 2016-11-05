using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Folding
{
    [Flags]
    internal enum FoldingStates : byte
    {
        None = 0x00,

        Invisible = 0x01,

        Header = 0x02
    }

    internal static class VisibleStatesExtensions
    {
        public static bool Has(this FoldingStates enu, FoldingStates flag) => (enu & flag) == flag;
    }
}
