using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.ObjectModel
{
    public sealed class Selection : Range
    {
        internal Selection()
        {

        }

        internal Selection(Pos caret) : this(caret, caret)
        {

        }

        internal Selection(Pos start, Pos end) : base(start, end.IsEmpty ? start : end)
        {
            
        }

        public static Selection FromRange(Range range)
        {
            return new Selection(range.Start, range.End);
        }

        internal void Clear()
        {
            Start = End;
        }

        internal void Clear(Pos pos)
        {
            Start = End = pos;
        }

        internal void Update(Selection sel)
        {
            Start = sel.Start;
            End = sel.End;
        }

        internal Selection Clone()
        {
            return (Selection)MemberwiseClone();
        }

        public Pos Caret
        {
            get { return End; }
        }
    }
}
