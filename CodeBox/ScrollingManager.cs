using System;
using CodeBox.ObjectModel;
using System.Drawing;
using CodeBox.Folding;

namespace CodeBox
{
    public sealed class ScrollingManager
    {
        private readonly Editor editor;
        private Point pointer;

        internal ScrollingManager(Editor editor)
        {
            this.editor = editor;
        }

        internal bool UpdateVisibleRectangle()
        {
            var caret = editor.Buffer.Selections.Main.Caret;

            if (caret.Line >= editor.Document.Lines.Count)
                return false;

            var sv = IsLineStripeVisible(caret);
            var update = false;

            if (sv != 0)
            {
                var sign = Math.Sign(sv);

                if (Math.Abs(sv) > 1 && caret.Line + sign < editor.Lines.Count
                    && sign < 0 && IsLineStripeVisible(new Pos(caret.Line + sign, caret.Col)) == 0)
                    sv = sign;

                ScrollY(sv);
                update = true;
            }

            if (!editor.Buffer.WordWrap)
            {
                sv = IsColumnVisible(caret);

                if (sv != 0)
                {
                    var sign = Math.Sign(sv);

                    if (Math.Abs(sv) > 10 && IsColumnVisible(new Pos(caret.Line, caret.Col + sign * 10)) == 0)
                        sv = sign * 10;

                    ScrollX(sv);
                    update = true;
                }
            }

            return update;
        }

        private int IsColumnVisible(Pos pos)
        {
            var tetras = editor.Lines[pos.Line].GetTetras(pos.Col, editor.Settings.TabSize);
            var curpos = tetras * editor.Info.CharWidth + editor.Info.TextLeft + X;

            if (curpos > editor.Info.TextRight)
            {
                curpos += editor.Info.CharWidth;
                var diff = curpos - editor.Info.TextRight;
                return -(int)(Math.Ceiling((double)diff / editor.Info.CharWidth));
            }
            else if (curpos < editor.Info.TextLeft)
                return (int)Math.Ceiling((editor.Info.TextLeft - curpos) / (double)editor.Info.CharWidth);
            else
                return 0;
        }

        private int IsLineStripeVisible(Pos pos)
        {
            var ln = editor.Document.Lines[pos.Line];
            var stripe = ln.GetStripe(pos.Col);
            var cy = editor.Info.TextTop + ln.Y + stripe * editor.Info.LineHeight + Y;

            if (cy < editor.Info.TextTop)
                return -(cy / editor.Info.LineHeight - 1);
            else if (cy + editor.Info.LineHeight > editor.Info.TextBottom)
                return -((cy - editor.Info.TextTop) / editor.Info.LineHeight);
            else
                return 0;
        }

        public void ScrollY(int times)
        {
            SetScrollPositionY(Y + times * editor.Info.LineHeight);
        }

        public void ScrollX(int times)
        {
            SetScrollPositionX(X + times * editor.Info.CharWidth);
        }

        public void SetScrollPositionY(int value)
        {
            if (value > 0)
                value = 0;

            if (value < -YMax)
                value = -YMax;

            //scroll by whole lines
            var lines = (int)Math.Round((double)value / editor.Info.LineHeight);
            value = lines * editor.Info.LineHeight;
            Y = value;
            ResetFirstLast();
            OnScroll();
        }

        public void SetScrollPositionX(int value)
        {
            if (value > 0)
                value = 0;

            if (value < -XMax)
                value = -XMax;

            //scroll by whole chars
            var chars = (int)Math.Round((double)value / editor.Info.CharWidth);
            value = chars * editor.Info.CharWidth;

            X = value;
            OnScroll();
        }

        private void OnScroll()
        {
            editor.Styles.Restyle();
            editor.Redraw();
        }

        private int CalculateFirstVisibleLine()
        {
            var stripes = Math.Abs(Y / editor.Info.LineHeight);

            if (stripes == 0)
                return -1;

            var len = editor.Document.Lines.Count;
            var cs = 0;

            for (var i = 0; i < len; i++)
            {
                var ln = editor.Lines[i];

                if (ln.Folding.Has(FoldingStates.Invisible))
                    continue;

                cs += ln.Stripes;

                if (cs == stripes)
                    return i;
                else if (cs > stripes)
                    return i - 1;
            }

            return len - 1;
        }

        private int? CalculateLastVisibleLine()
        {
            var len = editor.Document.Lines.Count;
            var lh = editor.Info.LineHeight;
            var maxh = editor.Info.TextBottom - editor.Info.TextTop - Y;

            for (var i = FirstVisibleLine; i < len; i++)
            {
                var ln = editor.Document.Lines[i];

                if (ln.Folding.Has(FoldingStates.Invisible))
                    continue;

                var lnEnd = ln.Y + ln.Stripes * lh;

                if (ln.Y >= maxh)
                    return i > FirstVisibleLine ? i - 1 : i;
            }

            return len > FirstVisibleLine ? (int?)len - 1 : null;
        }

        public void FastInvalidateLines()
        {
            var ln = editor.Lines[editor.Buffer.Selections.Main.Caret.Line];
            var w = ln.GetTetras(editor.Settings.TabSize) * editor.Info.CharWidth
                - editor.Info.TextWidth + editor.Info.CharWidth * 5;

            if (w > YMax)
                YMax = w;
        }

        public void InvalidateLines(bool force = false)
        {
            var dt = DateTime.Now;
            if (!editor.Buffer.WordWrap)
            {
                if (editor.AtomicChange && !force)
                {
                    FastInvalidateLines();
                    return;
                }

                var maxWidth = 0;
                var y = 0;

                foreach (var ln in editor.Document.Lines)
                {
                    if (ln.Folding.Has(FoldingStates.Invisible))
                        continue;

                    ln.Y = y;
                    var w = ln.GetTetras(editor.Settings.TabSize) * editor.Info.CharWidth;
                    y += editor.Info.LineHeight;

                    if (w > maxWidth)
                        maxWidth = w;
                }

                XMax = maxWidth - editor.Info.TextWidth + editor.Info.CharWidth * 5;
                XMax = XMax < 0 ? 0 : XMax;
                YMax = editor.Document.Lines.Count * editor.Info.LineHeight;
            }
            else
            {
                var maxHeight = 0;
                var twidth = editor.Info.TextWidth;

                foreach (var ln in editor.Document.Lines)
                {
                    if (ln.Folding.Has(FoldingStates.Invisible))
                        continue;

                    if (!ln.Invalidated || force)
                        ln.RecalculateCuts(twidth, editor.Info.CharWidth, editor.Settings.TabSize);
                    ln.Y = maxHeight;
                    maxHeight += ln.Stripes * editor.Info.LineHeight;
                }

                XMax = 0;
                YMax = maxHeight;
                ResetFirstLast();
            }

            YMax = YMax - editor.Info.TextHeight + editor.Info.LineHeight * 5;
            YMax = YMax < 0 ? 0 : YMax;

            _lastVisibleLine = null;
            Console.WriteLine($"FirstVisible: {FirstVisibleLine}; LastVisible: {LastVisibleLine}; InvalidateLines: {DateTime.Now - dt}");
        }

        private void ResetFirstLast()
        {
            _firstVisibleLine = null;
            _lastVisibleLine = null;
        }

        internal void OnPointerDown(Point loc)
        {
            pointer = loc;
        }

        internal void OnPointerUp(Point loc)
        {
            pointer = default(Point);
        }

        internal void OnPointerUpdate(Point loc)
        {
            var diffY = Math.Abs(loc.Y - pointer.Y);
            var diffX = Math.Abs(loc.X - pointer.X);

            if (diffY < editor.Info.LineHeight && diffX < editor.Info.CharWidth)
                return;

            if (diffY > diffX)
                ScrollY((loc.Y - pointer.Y) / editor.Info.LineHeight);
            else
                ScrollX((loc.X - pointer.X) / editor.Info.CharWidth);

            pointer = loc;
        }

        private int? _firstVisibleLine;
        public int FirstVisibleLine
        {
            get
            {
                if (_firstVisibleLine == null)
                    _firstVisibleLine = CalculateFirstVisibleLine() + 1;

                return _firstVisibleLine.Value;
            }
        }

        private int? _lastVisibleLine;
        public int LastVisibleLine
        {
            get
            {
                if (_lastVisibleLine == null)
                    _lastVisibleLine = CalculateLastVisibleLine();

                return _lastVisibleLine != null ? _lastVisibleLine.Value : 0;
            }
        }

        private int _y;
        public int Y
        {
            get { return _y; }
            set
            {
                _y = value;
            }
        }

        public int X { get; private set; }

        internal int XMax { get; set; }

        internal int YMax { get; set; }
    }
}
