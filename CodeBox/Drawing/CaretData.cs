using System;

namespace Slot.Editor.Drawing
{
    internal struct CaretData
    {
        public CaretData(int x, int y, int line, int col, bool blink)
        {
            X = x;
            Y = y;
            Line = line;
            Col = col;
            Blink = blink;
        }

        public readonly int X;
        public readonly int Y;
        public readonly int Line;
        public readonly int Col;
        public readonly bool Blink;
    }
}
