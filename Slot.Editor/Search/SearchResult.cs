using Slot.Editor.Styling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slot.Editor.Search
{
    public struct SearchResult
    {
        internal SearchResult(int line, AppliedStyle style)
        {
            Line = line;
            Style = style;
        }

        public int Line { get; }

        public int StartCol => Style.Start;

        public int EndCol => Style.End;

        internal AppliedStyle Style { get; }
    }
}
