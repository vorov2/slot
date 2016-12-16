using System;
using Slot.Editor.ObjectModel;

namespace Slot.Editor.Folding
{
    public sealed class FoldingNeededEventArgs : EventArgs
    {
        internal FoldingNeededEventArgs(Range range)
        {
            Range = range;
        }

        public Range Range { get; }
    }
}
