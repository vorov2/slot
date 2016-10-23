using CodeBox.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
