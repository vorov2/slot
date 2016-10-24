using System;

namespace CodeBox.Styling
{
    internal struct AppliedStyle
    {
        public AppliedStyle(int styleId, int start, int end)
        {
            StyleId = styleId;
            Start = start;
            End = end;
        }

        public readonly int StyleId;

        public readonly int Start;

        public readonly int End;
    }
}
