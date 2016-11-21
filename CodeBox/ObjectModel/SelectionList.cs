using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.ObjectModel
{
    public sealed class SelectionList : IEnumerable<Selection>
    {
        private readonly List<Selection> sels;
        private readonly Document doc;

        internal SelectionList(Document doc)
        {
            this.doc = doc;
            sels = new List<Selection>();
            sels.Add(new Selection());
        }

        internal void ValidateCaret(Selection sel)
        {
            var caret = sel.Caret;
            var len = sels.Count;

            if (caret.Col < 0 || caret.Line < 0
                || (caret.Line == doc.Lines.Count - 1 && caret.Col > doc.Lines[caret.Line].Length))
                Remove(sel);
            else if (len > 1)
            {
                for (var i = 0; i < len; i++)
                {
                    var s = sels[i];

                    if (s != sel && s.Caret.Line == caret.Line && s.Caret.Col == caret.Col)
                    {
                        Remove(s);
                        len--;
                        i--;
                    }
                }
            }
        }

        public void Add(Selection sel)
        {
            AddFast(sel);

            if (sel.IsEmpty)
                ValidateCaret(sel);
            else
            {
                var osel = GetIntersection(sel);

                if (osel != null)
                    Remove(osel);
            }
        }

        internal void AddFast(Selection sel) => sels.Add(sel);

        internal void AddFirst(Selection sel) => sels.Insert(0, sel);

        public void Set(Pos caret) => Set(new Selection(caret));

        public void Set(Selection sel)
        {
            if (sels.Count == 1)
                sels[0] = sel;
            else
            {
                sels.Clear();
                sels.Add(sel);
            }
        }

        internal void Clear() => sels.Clear();

        public void Truncate()
        {
            Set(Main);
        }

        public void Remove(Selection sel)
        {
            if (sels.Count > 1)
                sels.Remove(sel);
            else
                sel.Clear(sel.Caret);
        }

        public IEnumerator<Selection> GetEnumerator() => sels.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => sels.GetEnumerator();

        internal Selection GetIntersection(Selection except)
        {
            foreach (var r in sels)
                if (r != except && (r.InRange(except.Caret) || except.InRange(r.Caret)))
                    return r;

            return null;
        }

        public bool IsSelected(Pos pos)
        {
            foreach (var r in sels)
                if (!r.IsEmpty && r.InRange(pos))
                    return true;

            return false;
        }

        public bool HasCaret(Pos pos) => IndexOfCaret(pos) != -1;

        internal int IndexOfCaret(Pos pos)
        {
            for (var i = 0; i < sels.Count; i++)
                if (sels[i].Caret == pos)
                    return i;

            return -1;
        }

        public bool HasNonEmpty()
        {
            for (var i = 0; i < sels.Count; i++)
                if (sels[i].IsEmpty)
                    return false;

            return true;
        }

        public bool IsLineSelected(int lineIndex)
        {
            foreach (var s in sels)
            {
                var start = s.Start;
                var end = s.End;

                if (start > end)
                {
                    end = start;
                    start = s.End;
                }

                if (!s.IsEmpty && lineIndex >= start.Line && lineIndex <= end.Line)
                    return true;
            }

            return false;
        }

        public Selection this[int index] => sels[index];

        public Selection Main => sels[0];

        public int Count => sels.Count;
    }
}
