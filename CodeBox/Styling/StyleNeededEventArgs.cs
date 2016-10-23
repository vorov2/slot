using System;
using CodeBox.ObjectModel;

namespace CodeBox.Styling
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
