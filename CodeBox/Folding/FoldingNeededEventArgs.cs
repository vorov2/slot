using System;
using CodeBox.ObjectModel;

namespace CodeBox.Folding
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
