using System;
using CodeBox.ObjectModel;
using System.Drawing;

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
            var count = 0;
            var stripe = editor.Lines[pos.Line].GetStripe(pos.Col);

            if (pos.Line >= LastVisibleLine)
            {
                for (var i = LastVisibleLine; ; i++)
                {
                    var ln = editor.Lines[i];
                    
                    for (var j = 0; j < ln.Stripes; j++)
                    {
                        if (pos.Line == i && stripe == j)
                            return count;
                        count--;
                    }
                }
            }
            else if (pos.Line <= FirstVisibleLine)
            {
                for (var i = FirstVisibleLine; ; i--)
                {
                    var ln = editor.Lines[i];

                    for (var j = 0; j < ln.Stripes; j++)
                    {
                        if (pos.Line == i && stripe == j)
                            return count;
                        count++;
                    }
                }
            }

            return count;
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
            var delta = Y / editor.Info.LineHeight - lines;
            var fvl = CalculateFirstVisibleLine(delta);

            if (fvl >= editor.Lines.Count)
                fvl = editor.Lines.Count - 1;

            FirstVisibleLine = fvl;
            value = lines * editor.Info.LineHeight;

            Y = value;
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

        private int CalculateFirstVisibleLine(int delta)
        {
            if (delta == 0)
                return FirstVisibleLine;

            var stripes = 0;
            var sign = Math.Sign(delta);
            var abs = Math.Abs(delta);
            var ret = 0;
            var max = delta > 0 ? FirstVisibleLine + delta + 1 : FirstVisibleLine + delta - 1;
            max = max >= editor.Lines.Count ? editor.Lines.Count - 1
                : max < 0 ? 0 : max;

            for (var i = FirstVisibleLine + sign
                ; delta > 0 ? i < max : i > max
                ; i += sign)
            {
                stripes += editor.Lines[i].Stripes;

                if (stripes > abs)
                {
                    ret = i - 1;
                    break;
                }
                else if (stripes == abs)
                {
                    ret = i;
                    break;
                }
            }

            return ret < 0 ? 0 : ret;
        }

        private int? CalculateLastVisibleLine()
        {
            var len = editor.Document.Lines.Count;
            var lh = editor.Info.LineHeight;
            var maxh = editor.Info.TextBottom;
            var y = editor.Info.TextTop;

            for (var i = FirstVisibleLine; i < len; i++)
            {
                var ln = editor.Document.Lines[i];

                if (y >= maxh)
                    return i > FirstVisibleLine ? i - 1 : i;

                y += ln.Stripes * editor.Info.LineHeight;
            }

            return len > FirstVisibleLine ? (int?)len - 1 : null;
        }

        public void InvalidateLines()
        {
            if (!editor.Buffer.WordWrap)
            {
                var maxWidth = 0;
                var y = 0;

                foreach (var ln in editor.Document.Lines)
                {
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
                    ln.RecalculateCuts(twidth, editor.Info.CharWidth, editor.Settings.TabSize);
                    maxHeight += ln.Stripes * editor.Info.LineHeight;
                }

                XMax = 0;
                YMax = maxHeight;
            }

            YMax = YMax - editor.Info.TextHeight + editor.Info.LineHeight * 5;
            YMax = YMax < 0 ? 0 : YMax;

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

        private int _firstVisibleLine;
        public int FirstVisibleLine
        {
            get { return _firstVisibleLine; }
            set
            {
                _firstVisibleLine = value;
                _lastVisibleLine = null;
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

        public int Y { get; private set; }

        public int X { get; private set; }

        internal int XMax { get; set; }

        internal int YMax { get; set; }
    }
}
