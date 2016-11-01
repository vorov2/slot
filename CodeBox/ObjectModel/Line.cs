using CodeBox.Folding;
using CodeBox.Styling;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.ObjectModel
{
    public sealed class Line : IEnumerable<Character>
    {
        private List<Character> chars;
        private List<int> cuts;

        public Line(IEnumerable<Character> chars, int id)
        {
            Id = id;
            this.chars = chars != null ? new List<Character>(chars) : new List<Character>();
        }

        private Line(char[] chars, int id)
        {
            Id = id;
            this.chars = new List<Character>(chars.Select(c => new Character(c)));
        }

        public static Line FromString(string line, int id)
        {
            return string.IsNullOrEmpty(line) ? new Line(new char[0], id) : new Line(line.ToCharArray(), id);
        }

        internal bool TrailingCaret { get; set; }

        public int Id { get; private set; }

        internal int Y { get; set; }
        
        public int Length
        {
            get { return chars.Count; }
        }

        public byte State { get; set; }

        public string Text
        {
            get
            {
                return chars.MakeString();
            }
        }

        public override string ToString()
        {
            return Text;
        }

        public override bool Equals(object obj)
        {
            var ln = obj as Line;
            return ln != null && ln.Id == Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public IEnumerator<Character> GetEnumerator()
        {
            return ((IEnumerable<Character>)chars).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return chars.GetEnumerator();
        }

        #region Text Modification
        public char CharAt(int index)
        {
            return chars.Count > index ? chars[index].Char : '\0';
        }

        public Character CharacterAt(int index)
        {
            return chars[index];
        }

        public void Append(IEnumerable<Character> str)
        {
            chars.AddRange(str);
            Invalidated = false;
            _tetras = -1;
        }

        public void Insert(int index, Character ch)
        {
            if (index >= chars.Count)
                chars.Add(ch);
            else if (index >= 0)
                chars.Insert(index, ch);
            _tetras = -1;
            Invalidated = false;
        }

        public void Insert(int index, IEnumerable<Character> str)
        {
            if (index >= chars.Count)
                Append(str);
            else if (index >= 0)
                chars.InsertRange(index, str);
            _tetras = -1;
            Invalidated = false;
        }

        public void RemoveRange(int index, int count)
        {
            if (index + count > Length)
                count = Length - index;

            chars.RemoveRange(index, count);
            _tetras = -1;
            Invalidated = false;
        }

        public List<Character> GetRange(int index, int count)
        {
            return chars.GetRange(index, count);
        }

        public void RemoveAt(int index)
        {
            chars.RemoveAt(index);
            _tetras = -1;
            Invalidated = false;
        }

        public Character this[int index]
        {
            get { return CharacterAt(index); }
            set
            {
                if (index < Length)
                    chars[index] = value;
                else
                    chars.Add(value);
                _tetras = -1;
                Invalidated = false;
            }
        }
        #endregion

        #region Styles
        internal readonly List<AppliedStyle> AppliedStyles = new List<AppliedStyle>();

        public AppliedStyle FindHyperlink(int col)
        {
            foreach (var a in AppliedStyles)
            {
                if (col >= a.Start && col <= a.End && a.StyleId == (int)StandardStyle.Hyperlink)
                    return a;
            }

            return AppliedStyle.Empty;
        }

        internal Style GetStyle(int index, StyleManager man)
        {
            var ret = default(Style);

            foreach (var a in AppliedStyles)
            {
                if (index >= a.Start && index <= a.End)
                {
                    if (ret == null)
                        ret = man.GetStyle(a.StyleId);
                    else
                    {
                        var next = man.GetStyle(a.StyleId);
                        ret = ret.Combine(next);
                    }
                }
            }

            ret = ret ?? man.GetStyle((int)StandardStyle.Default);
            return ret;
        }
        #endregion

        #region Tetras, cuts and Stripes
        internal int GetTetras(int pos, int tabSize)
        {
            var tetra = 0;
            var max = pos > chars.Count ? chars.Count : pos;

            for (var i = 0; i < max; i++)
            {
                var c = chars[i];
                tetra += c.Char == '\t' ? tabSize : 1;
            }

            return tetra;
        }

        internal void ClearCuts()
        {
            Invalidated = false;
            _tetras = -1;
            cuts = null;
        }

        internal void RecalculateCuts(int limit, int charWidth, int tabSize)
        {
            if (cuts != null)
                cuts.Clear();

            var width = 0;

            for (var i = 0; i < chars.Count; i++)
            {
                var c = chars[i];
                var w = c.Char == '\t' ? tabSize * charWidth : charWidth;

                width += w;

                if (c.Char == ' ' || c.Char == '\t')
                {
                    var tet = GetNextWordTetras(i + 1, tabSize);

                    if (width + tet * charWidth > limit)
                    {
                        width = 0;
                        AddCut(i + 1);
                    }
                }
            }

            Invalidated = true;
        }

        private int GetNextWordTetras(int index, int tabSize)
        {
            var tetras = 0;

            for (var i = index; i < chars.Count; i++)
            {
                var c = chars[i];

                if (c.Char == '\t' || c.Char == ' ')
                {
                    if (tetras == 0)
                        tetras = c.Char == '\t' ? tabSize : 1;

                    break;
                }

                tetras++;
            }

            return tetras;
        }

        internal int GetStripe(int col)
        {
            for (var i = 0; i < Stripes; i++)
            {
                var cut = GetCut(i);

                if (col <= cut)
                    return i;
            }

            return Stripes - 1;
        }

        internal int GetStripeCol(int col, int stripe)
        {
            var start = stripe > 0 ? GetCut(stripe - 1) + 1 : 0;
            return col - start;
        }

        private int _tetras = -1;
        internal int GetTetras(int tabSize)
        {
            if (_tetras != -1)
                return _tetras;

            var c = 0;

            for (var i = 0; i < chars.Count; i++)
                if (chars[i].Char == '\t')
                    c++;

            return _tetras = chars.Count - c + c * tabSize;
        }
        
        private void AddCut(int cut)
        {
            if (cuts == null)
                cuts = new List<int>();

            cuts.Add(cut);
        }

        public int GetCut(int i)
        {
            return cuts == null || i >= cuts.Count ? Length : cuts[i];
        }

        public int Stripes
        {
            get { return cuts == null ? 1 : cuts.Count + 1; }
        }

        internal bool Invalidated { get; private set; }
        #endregion

        #region Folding
        public FoldingStates Folding { get; set; } = FoldingStates.None;
        #endregion
    }
}
