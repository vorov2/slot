using System;
using Slot.Editor.ObjectModel;

namespace Slot.Editor.Styling
{
    public sealed class StyleNeededEventArgs : EventArgs
    {
        internal StyleNeededEventArgs(Range range)
        {
            Range = range;
        }

        public Range Range { get; }
    }
}
