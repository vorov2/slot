using CodeBox.ObjectModel;
using System;
namespace CodeBox
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
