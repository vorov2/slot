using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;

namespace CodeBox
{
    public sealed class ScrollingManager
    {
        private readonly Editor editor;

        internal ScrollingManager(Editor editor)
        {
            this.editor = editor;
        }

        internal bool UpdateVisibleRectangle()
        {
            var caret = editor.Document.Selections.Main.Caret;

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

            if (!editor.Settings.WordWrap)
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
            var curpos = tetras * editor.Info.CharWidth + editor.Info.EditorLeft + X;

            if (curpos > editor.Info.EditorRight)
            {
                curpos += editor.Info.CharWidth;
                var diff = curpos - editor.Info.EditorRight;
                return -(int)(Math.Ceiling((double)diff / editor.Info.CharWidth));
            }
            else if (curpos < editor.Info.EditorLeft)
                return (int)Math.Ceiling((editor.Info.EditorLeft - curpos) / (double)editor.Info.CharWidth);
            else
                return 0;
        }
        
        private int IsLineStripeVisible(Pos pos)
        {
            var ln = editor.Document.Lines[pos.Line];
            var stripe = ln.GetStripe(pos.Col);
            var cy = editor.Info.EditorTop + ln.Y + stripe * editor.Info.LineHeight + Y;

            if (cy < editor.Info.EditorTop)
                return -(cy / editor.Info.LineHeight - 1);
            else if (cy + editor.Info.LineHeight > editor.Info.EditorBottom)//editor.ClientSize.Height - editor.Info.BottomMargin - editor.Info.EditorTop)
                return -((cy - editor.Info.EditorTop) / editor.Info.LineHeight);
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
            FirstVisibleLine = -lines;

            if (FirstVisibleLine >= editor.Lines.Count)
                FirstVisibleLine = editor.Lines.Count - 1;

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
            editor.Restyle();
            editor.Redraw();
        }

        private int? CalculateLastVisibleLine()
        {
            var len = editor.Document.Lines.Count;
            var lh = editor.Info.LineHeight;
            var maxh = editor.Info.EditorBottom - editor.Info.EditorTop - Y;

            for (var i = FirstVisibleLine; i < len; i++)
            {
                var ln = editor.Document.Lines[i];
                var lnEnd = ln.Y + ln.Stripes * lh;

                if (ln.Y >= maxh)
                    return i > FirstVisibleLine ? i - 1 : i;
            }

            return len > FirstVisibleLine ? (int?)len - 1 : null;
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
