using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slot.Editor.ObjectModel
{
    public sealed class Selection : Range
    {
        internal Selection()
        {

        }

        public Selection(Pos caret) : this(caret, caret)
        {

        }

        public Selection(Pos start, Pos end) : base(start, end.IsEmpty ? start : end)
        {

        }

        public static Selection FromRange(Range range) => new Selection(range.Start, range.End);

        internal void Clear() => Clear(End);

        internal void SetToRestore(int pos) => RestoreCaretCol = pos;

        internal void Clear(Pos pos) => Start = End = pos;

        internal void Update(Selection sel)
        {
            Start = sel.Start;
            End = sel.End;
        }

        internal Selection Clone() => (Selection)MemberwiseClone();

        internal int GetFirstLine() => Start > End ? End.Line : Start.Line;

        internal int GetLastLine() => Start > End ? Start.Line : End.Line;

        public Pos Caret => End;

        internal int RestoreCaretCol { get; private set; }
    }
}
