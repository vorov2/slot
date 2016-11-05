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

        public static Selection FromRange(Range range) => new Selection(range.Start, range.End);

        internal void Clear() => Clear(End);

        internal void SetToRestore() => RestoreCaretCol = Caret.Col;

        internal void SetToRestore(Pos caret) => RestoreCaretCol = caret.Col;

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

        public int RestoreCaretCol { get; private set; }
    }
}
