using System;

namespace Slot.Editor.Folding
{
    [Flags]
    internal enum FoldingStates : byte
    {
        None = 0x00,

        Invisible = 0x01,

        Header = 0x02
    }

    internal static class FoldingStatesExtensions
    {
        public static bool Has(this FoldingStates enu, FoldingStates flag) => (enu & flag) == flag;
    }
}
