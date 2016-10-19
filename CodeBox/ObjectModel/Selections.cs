using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.ObjectModel
{
    public sealed class Selections : IEnumerable<Selection>
    {
        private readonly List<Selection> sels;

        internal Selections()
        {
            sels = new List<Selection>();
            sels.Add(new Selection());
        }

        internal Selections(IEnumerable<Selection> sels)
        {
            this.sels = sels.ToList();
        }

        internal void Add(Selection sel)
        {
            sels.Add(sel);
        }

        internal void Set(Pos caret)
        {
            Set(new Selection(caret));
        }

        internal void Set(Selection sel)
        {
            if (sels.Count == 1)
                sels[0] = sel;
            else
            {
                sels.Clear();
                sels.Add(sel);
            }
        }

        internal void ForceClear()
        {
            sels.Clear();
        }

        internal void Clear()
        {
            if (sels.Count == 1)
                sels[0].Clear();
            else
            {
                var sel = sels[sels.Count - 1];
                sel.Clear();
                Set(sel);
            }
        }

        internal Selections Clone()
        {
            return new Selections(sels.Select(s => s.Clone()));
        }

        internal void Remove(Selection sel)
        {
            if (sels.Count > 1)
                sels.Remove(sel);
            else
                sel.Clear(new Pos(0, 0));
        }
        
        public IEnumerator<Selection> GetEnumerator()
        {
            return sels.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return sels.GetEnumerator();
        }

        internal Selection GetSelection(Pos pos, Selection except = null)
        {
            foreach (var r in sels)
                if (r.InRange(pos) && r != except)
                    return r;

            return null;
        }

        internal bool IsSelected(Pos pos)
        {
            foreach (var r in sels)
                if (!r.IsEmpty && r.InRange(pos))
                    return true;

            return false;
        }

        internal bool HasCaret(Pos pos)
        {
            return IndexOfCaret(pos) != -1;
        }

        internal int IndexOfCaret(Pos pos)
        {
            for (var i = 0; i < sels.Count; i++)
                if (sels[i].Caret == pos)
                    return i;

            return -1;
        }

        internal bool Any()
        {
            for (var i = 0; i < sels.Count; i++)
                if (sels[i].IsEmpty)
                    return false;

            return true;
        }
        
        internal bool IsLineSelected(int lineIndex)
        {
            foreach (var s in this)
            {
                var start = s.Start;
                var end = s.End;

                if (start > end)
                {
                    end = start;
                    start = s.Start;
                }

                if (!s.IsEmpty && lineIndex >= s.Start.Line && lineIndex <= s.End.Line)
                    return true;
            }

            return false;
        }

        internal Selection this[int index]
        {
            get { return sels[index]; }
        }
        
        public Selection Main
        {
            get { return sels[sels.Count - 1]; }
        }

        internal int TotalCount
        {
            get { return sels.Count; }
        }
    }
}
