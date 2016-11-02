using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Drawing
{
    internal struct CaretData
    {
        public CaretData(int x, int y, bool blink)
        {
            X = x;
            Y = y;
            Blink = blink;
        }

        public readonly int X;
        public readonly int Y;
        public readonly bool Blink;
    }
}
