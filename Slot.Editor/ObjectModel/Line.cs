using Slot.Editor.Affinity;
using Slot.Core.Themes;
using Slot.Editor.Folding;
using Slot.Editor.Styling;
using System;
using System.Collections.Generic;
using System.Linq;
using Slot.Editor.Drawing;
using Slot.Core;

namespace Slot.Editor.ObjectModel
{
    public sealed class Line : IEnumerable<Character>
    {
        private static readonly char[] emptyArray = new char[0];
        private List<Character> chars;
        private List<int> cuts;
        private int tetraCount = -1;
        internal readonly List<GrammarInfo> Grammars = new List<GrammarInfo>();

        public Line(IEnumerable<Character> chars)
        {
            this.chars = chars != null ? new List<Character>(chars) : new List<Character>();
        }

        private Line(char[] chars)
        {
            this.chars = new List<Character>(chars.Select(c => new Character(c)));
        }

        internal static int GetCharWidth(char c)
        {
            switch (char.GetUnicodeCategory(c))
            {
                case System.Globalization.UnicodeCategory.OtherLetter:
                case System.Globalization.UnicodeCategory.OtherNumber:
                case System.Globalization.UnicodeCategory.OtherNotAssigned:
                case System.Globalization.UnicodeCategory.OtherSymbol:
                    return 2;
                default: return 1;
            }
        }

        public static Line Empty() => new Line(emptyArray);

        public static Line FromString(string line) =>
            string.IsNullOrEmpty(line) ? new Line(emptyArray) : new Line(line.ToCharArray());

        internal bool TrailingCaret { get; set; }

        internal int Y { get; set; }

        public int Length => chars.Count;

        public int State { get; set; }

        public string Text => chars.MakeString();

        public override string ToString() => Text;

        public override bool Equals(object obj)
        {
            var ln = obj as Line;
            return ln != null && ln.chars == chars;
        }

        public override int GetHashCode() => chars.GetHashCode();

        public IEnumerator<Character> GetEnumerator()
        {
            return ((IEnumerable<Character>)chars).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return chars.GetEnumerator();
        }

        #region Text Modification
        public bool IsEmpty()
        {
            if (chars.Count == 0)
                return true;

            foreach (var c in chars)
                if (c.Char != ' ' && c.Char != '\t')
                    return false;

            return true;
        }

        public bool WhiteSpaceBefore(int col)
        {
            if (col < 0)
                throw new SlotException(
                    $"Negative value for the {nameof(col)} argument is not allowed.");

            for (var i = 0; i < col; i++)
                if (chars[i].Char != ' ')
                    return false;

            return true;
        }

        public int GetFirstNonIndentChar()
        {
            for (var i = 0; i < Length; i++)
                if (chars[i].Char != ' ' && chars[i].Char != '\t')
                    return i;

            return 0;
        }

        public char CharAt(int index) => chars.Count > index ? chars[index].Char : '\0';

        public Character CharacterAt(int index) => chars[index];

        public void Append(IEnumerable<Character> str)
        {
            chars.AddRange(str);
            Invalidated = false;
            tetraCount = -1;
        }

        public void Insert(int index, Character ch)
        {
            if (index >= chars.Count)
                chars.Add(ch);
            else if (index >= 0)
                chars.Insert(index, ch);
            tetraCount = -1;
            Invalidated = false;
        }

        public void Insert(int index, IEnumerable<Character> str)
        {
            if (index >= chars.Count)
                Append(str);
            else if (index >= 0)
                chars.InsertRange(index, str);
            tetraCount = -1;
            Invalidated = false;
        }

        public void RemoveRange(int index, int count)
        {
            if (index + count > Length)
                count = Length - index;

            chars.RemoveRange(index, count);
            tetraCount = -1;
            Invalidated = false;
        }

        public List<Character> GetRange(int index, int count) =>
            chars.GetRange(index, count);

        public void RemoveAt(int index)
        {
            chars.RemoveAt(index);
            tetraCount = -1;
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
                tetraCount = -1;
                Invalidated = false;
            }
        }

        internal void Reset()
        {
            tetraCount = -1;
            Invalidated = false;
        }
        #endregion

        #region Styles
        internal readonly List<AppliedStyle> AppliedStyles = new List<AppliedStyle>();

        public AppliedStyle FindHyperlink(int col)
        {
            foreach (var a in AppliedStyles)
            {
                if (col >= a.Start && col <= a.End
                    && a.StyleId == StandardStyle.Hyperlink)
                    return a;
            }

            return AppliedStyle.Empty;
        }

        internal Style GetStyle(int index, ITheme theme)
        {
            var ret = Style.Empty;

            foreach (var a in AppliedStyles)
            {
                if (index >= a.Start && index <= a.End)
                {
                    var next = theme.GetStyle(a.StyleId);
                    ret = ret.Combine(next);
                }
            }

            return ret.IsEmpty() ? StyleRenderer.DefaultStyle : ret;
        }
        #endregion

        #region Tetras, cuts and Stripes
        public int GetTetras(int pos, int tabSize)
        {
            var tetra = 0;
            var max = pos > chars.Count ? chars.Count : pos;

            for (var i = 0; i < max; i++)
            {
                var c = chars[i];
                tetra += c.Char == '\t' ? GetIndentationSize(tetra, tabSize) : GetCharWidth(c.Char);
            }

            return tetra;
        }

        internal void ClearCuts()
        {
            Invalidated = false;
            tetraCount = -1;
            Indent = -1;
            cuts = null;
        }

        internal int GetLineIndent() => Indent;

        internal void RecalculateCuts(int limit, int charWidth, int tabSize, WrappingIndent win)
        {
            if (cuts != null)
                cuts.Clear();

            var width = 0;
            var tetra = 0;
            var indent = false;

            for (var i = 0; i < chars.Count; i++)
            {
                var c = chars[i];
                var ct = c.Char == '\t' ? GetIndentationSize(tetra, tabSize) : GetCharWidth(c.Char);
                tetra += ct;
                var w = ct * charWidth;

                width += w;

                if (!indent && (cuts == null || cuts.Count == 0) && c.Char != ' ' && c.Char != '\t')
                {
                    indent = true;
                    Indent = win == WrappingIndent.None ? 0
                        : win == WrappingIndent.Same ? tetra - 1
                        : tetra - 1 + tabSize;
                }

                if (IsSep(c.Char))
                {
                    var tet = GetNextWordTetras(i + 1, tabSize);

                    if (width + tet * charWidth > limit)
                    {
                        width = Indent * charWidth;
                        AddCut(i + 1);
                    }
                }
            }

            Invalidated = true;
        }

        private bool IsSep(char ch) =>
            ch == ' ' || ch == '\t' || ch == ' ' || ch == '.' || ch == '!'
                || ch == '?' || ch == ',' || ch == ';' || ch == '(' || ch == ')';

        private int GetNextWordTetras(int index, int tabSize)
        {
            var tetras = 0;

            for (var i = index; i < chars.Count; i++)
            {
                var c = chars[i];
                var ct = c.Char == '\t' ? GetTetras(i, tabSize) : GetCharWidth(c.Char);
                tetras += ct;

                if (IsSep(c.Char))
                    break;
            }

            return tetras;
        }

        internal int GetStripe(int col)
        {
            for (var i = 0; i < Stripes; i++)
            {
                var cut = GetCut(i);

                if (col < cut)
                    return i;
            }

            return Stripes - 1;
        }

        internal int GetStripeCol(int col) => GetStripeCol(col, GetStripe(col));

        internal int GetStripeCol(int col, int stripe)
        {
            var start = stripe > 0 ? GetCut(stripe - 1) : 0;
            return col - start;
        }

        public int GetTetras(int tabSize)
        {
            if (tetraCount != -1)
                return tetraCount;

            var c = 0;

            for (var i = 0; i < chars.Count; i++)
                if (chars[i].Char == '\t')
                    c += GetIndentationSize(c, tabSize);
                else
                    c += GetCharWidth(chars[i].Char);

            return tetraCount = c;
        }

        internal int GetColumnForTetra(int tetra, int tabSize)
        {
            if (tetra == 0)
                return 0;

            var oldtetra = 0;

            for (var i = 0; i < chars.Count; i++)
            {
                var c = chars[i].Char;
                oldtetra += c == '\t' ? GetIndentationSize(oldtetra, tabSize) : GetCharWidth(c);

                if (oldtetra >= tetra)
                    return i+1;
            }

            return tetra;
        }

        internal static int GetIndentationSize(int tet, int tabSize) => tabSize - tet % tabSize;

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

        internal int Indent { get; private set; }
        #endregion

        #region Folding
        internal FoldingStates Folding { get; set; } = FoldingStates.None;

        internal byte FoldingLevel { get; set; }
        #endregion
    }
}
