using CodeBox.Drawing;
using CodeBox.Styling;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeBox.Margins
{
    public sealed class ScrollBarMargin : Margin
    {
        private bool mouseDown;
        private int lastCaretPos;
        private int lastCaretSize;
        private int diff;

        public ScrollBarMargin(Editor editor, Orientation orientation) : base(editor)
        {
            Orientation = orientation;
        }

        public override MarginEffects MouseDown(Point loc)
        {
            if (IsCaretInLocation(loc))
            {
                mouseDown = true;
                diff = (Orientation == Orientation.Horizontal ? loc.X : loc.Y) - lastCaretPos;
                return MarginEffects.Redraw | MarginEffects.CaptureMouse;
            }
            else
            {
                var value = GetScrollValue(loc, true);
                SetScrollPosition(value);
                return MarginEffects.Redraw;
            }
        }

        public override MarginEffects MouseUp(Point loc)
        {
            mouseDown = false;
            return MarginEffects.Redraw;
        }

        public override MarginEffects MouseMove(Point loc)
        {
            if (mouseDown)
            {
                var value = GetScrollValue(loc, false);
                SetScrollPosition(value);
                return MarginEffects.Redraw;
            }

            return MarginEffects.None;
        }

        private int GetScrollValue(Point loc, bool inCaret)
        {
            long max = GetMaximum();
            var v = Orientation == Orientation.Horizontal ? loc.X - Bounds.X : loc.Y - Bounds.Y;
            v -= diff;
            var scrollSize = v / ((double)GetScrollSize() - lastCaretSize);
            var ret = -(max * scrollSize);
            
            if (ret > 0)
                ret = 0;
            else if (ret < -max)
                ret = -max;
            
            return (int)ret;
        }

        protected override bool OnDraw(Graphics g, Rectangle bounds)
        {
            Enabled = CalculateSize() != 0;

            if (!Enabled)
                return false;

            var sc = new Rectangle(Editor.Scroll.ScrollPosition, Editor.Scroll.ScrollBounds);
            g.FillRectangle(Editor.Settings.ScrollBackColor.Brush(), bounds);

            if (Orientation == Orientation.Horizontal)
            {
                var caretSize = ((double)bounds.Width / (bounds.Width + sc.Width)) * bounds.Width;

                if (caretSize < Editor.Info.CharWidth*3)
                    caretSize = Editor.Info.CharWidth*3;

                var perc = (bounds.Width - caretSize) / 100d;
                var curpos = sc.Width != 0 ? (int)Math.Floor(Math.Floor(sc.X / (sc.Width / 100d)) * perc) : 0;

                lastCaretSize = (int)Math.Floor(caretSize);
                g.FillRectangle((mouseDown ? Editor.Settings.ScrollActiveForeColor : Editor.Settings.ScrollForeColor).Brush(),
                    new Rectangle(bounds.X + Math.Abs(curpos), bounds.Y, lastCaretSize, bounds.Height));
                lastCaretPos = bounds.X + Math.Abs(curpos);
            }
            else
            {
                var caretSize = ((double)bounds.Height / (bounds.Height + sc.Height)) * bounds.Height;

                if (caretSize < Editor.Info.LineHeight)
                    caretSize = Editor.Info.LineHeight;

                var perc = (bounds.Height - caretSize) / 100d;
                var curpos = sc.Height != 0 ? (int)Math.Floor(Math.Floor(sc.Y / (sc.Height / 100d)) * perc) : 0;

                lastCaretSize = (int)Math.Floor(caretSize);
                var pos = bounds.Y + Math.Abs(curpos);

                if (pos + lastCaretSize > Editor.ClientSize.Height)
                    pos = Editor.ClientSize.Height - lastCaretSize;

                g.FillRectangle((mouseDown ? Editor.Settings.ScrollActiveForeColor 
                    : Editor.Settings.ScrollForeColor).Brush(),
                    new Rectangle(bounds.X, pos, bounds.Width, lastCaretSize));
                lastCaretPos = pos;
                var caretLine = Editor.Buffer.Selections.Main.Caret.Line;

                foreach (var s in Editor.Buffer.Selections)
                {
                    var linePos = s.Caret.Line / (Editor.Lines.Count / 100d);
                    var caretY = Editor.Info.TextTop + linePos * (bounds.Height / 100d);

                    g.FillRectangle(Editor.Styles.Default.ForeColor.Brush(), new Rectangle(bounds.X, (int)caretY, bounds.Width,
                        (int)Math.Round(g.DpiY / 96f) * s.Caret.Line == caretLine ? 2 : 1));
                }
            }

            return true;
        }

        public override int CalculateSize()
        {
            if (Orientation == Orientation.Horizontal)
            {
                if (Editor.Scroll.ScrollBounds.Width == 0)
                    return 0;
                else
                    return Editor.Info.CharWidth;
            }
            else
            {
                if (Editor.Scroll.ScrollBounds.Height == 0)
                    return 0;
                else
                    return (int)(Editor.Info.CharWidth*1.5);
            }
        }

        private bool IsCaretInLocation(Point loc)
        {
            if (Orientation == Orientation.Horizontal)
                return loc.X >= lastCaretPos && loc.X < lastCaretPos + lastCaretSize;
            else
                return loc.Y >= lastCaretPos && loc.Y < lastCaretPos + lastCaretSize;
        }

        private void SetScrollPosition(int value)
        {
            if (Orientation == Orientation.Horizontal)
                Editor.Scroll.SetScrollPositionX(value);
            else
                Editor.Scroll.SetScrollPositionY(value);
        }

        private int GetScrollSize()
        {
            if (Orientation == Orientation.Horizontal)
                return Editor.Info.TextWidth;
            else
                return Editor.Info.TextHeight;
        }

        private int GetMaximum()
        {
            if (Orientation == Orientation.Horizontal)
                return Editor.Scroll.ScrollBounds.Width;
            else
                return Editor.Scroll.ScrollBounds.Height;
        }

        public Orientation Orientation { get; set; }

        private bool _enabled;
        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (value != _enabled)
                {
                    _enabled = value;
                    OnSizeChanged();
                }
            }
        }
    }
}
