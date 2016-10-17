using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.ObjectModel
{
    public class Range
    {
        internal Range()
        {

        }

        public Range(Pos start, Pos end)
        {
            Start = start;
            End = end;
        }

        public Pos Start { get; set; }

        public Pos End { get; set; }

        public bool IsEmpty
        {
            get { return Start == End; }
        }

        internal Range Normalize()
        {
            var ret = new Range();
            ret.Start = Start;
            ret.End = End;

            if (Start > End)
            {
                var start = ret.Start;
                ret.Start = End;
                ret.End = start;
            }

            return ret;
        }

        public bool InRange(Pos pos)
        {
            Pos start = Start, end = End;

            if (Start > End)
            {
                start = End;
                end = Start;
            }

            return pos >= start && pos < end;
        }

        public override string ToString()
        {
            return String.Format("Start:({0});End:({1})", Start, End);
        }
    }
}
