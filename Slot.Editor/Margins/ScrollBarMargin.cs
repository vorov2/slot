using Slot.Core.Themes;
using Slot.Drawing;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Slot.Editor.Margins
{
    public class ScrollBarMargin : Margin
    {
        private int diff;

        public ScrollBarMargin(EditorControl editor, Orientation orientation) : base(editor)
        {
            Orientation = orientation;
        }

        public override void Reset()
        {
            LastCaretPos = 0;
            LastCaretSize = 0;
            IsMouseDown = false;
            diff = 0;
        }

        public override MarginEffects MouseDown(Point loc)
        {
            if (IsCaretInLocation(loc))
            {
                IsMouseDown = true;
                diff = (Orientation == Orientation.Horizontal ? loc.X : loc.Y) - LastCaretPos;
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
            IsMouseDown = false;
            return MarginEffects.Redraw;
        }

        public override MarginEffects MouseMove(Point loc)
        {
            if (IsMouseDown)
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
            var scrollSize = v / ((double)GetScrollSize() - LastCaretSize);
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
            var sbs = Editor.Theme.GetStyle(StandardStyle.ScrollBars);
            var asbs = Editor.Theme.GetStyle(StandardStyle.ActiveScrollBar);
            //g.FillRectangle(sbs.BackColor.Brush(), bounds);

            if (Orientation == Orientation.Horizontal)
            {
                var caretSize = ((double)bounds.Width / (bounds.Width + sc.Width)) * bounds.Width;

                if (caretSize < Editor.Info.CharWidth*3)
                    caretSize = Editor.Info.CharWidth*3;

                var perc = (bounds.Width - caretSize) / 100d;
                var curpos = sc.Width != 0 ? (int)Math.Floor(Math.Floor(sc.X / (sc.Width / 100d)) * perc) : 0;

                LastCaretSize = (int)Math.Floor(caretSize);
                g.FillRectangle((IsMouseDown ? asbs.ForeColor : sbs.ForeColor).Brush(),
                    new Rectangle(bounds.X + Math.Abs(curpos), bounds.Y, LastCaretSize, bounds.Height));
                LastCaretPos = bounds.X + Math.Abs(curpos);
            }
            else
            {
                var caretSize = ((double)bounds.Height / (bounds.Height + sc.Height)) * bounds.Height;

                if (caretSize < Editor.Info.LineHeight)
                    caretSize = Editor.Info.LineHeight;

                var perc = (bounds.Height - caretSize) / 100d;
                var curpos = sc.Height != 0 ? (int)Math.Floor(Math.Floor(sc.Y / (sc.Height / 100d)) * perc) : 0;

                LastCaretSize = (int)Math.Floor(caretSize);
                var pos = bounds.Y + Math.Abs(curpos);

                if (pos + LastCaretSize > Editor.ClientSize.Height)
                    pos = Editor.ClientSize.Height - LastCaretSize;

                g.FillRectangle((IsMouseDown ? asbs.ForeColor : sbs.ForeColor).Brush(),
                    new Rectangle(bounds.X, pos, bounds.Width, LastCaretSize));
                LastCaretPos = pos;
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
                    return Editor.Info.CharWidth;
            }
        }

        private bool IsCaretInLocation(Point loc)
        {
            if (Orientation == Orientation.Horizontal)
                return loc.X >= LastCaretPos && loc.X < LastCaretPos + LastCaretSize;
            else
                return loc.Y >= LastCaretPos && loc.Y < LastCaretPos + LastCaretSize;
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

        protected bool IsMouseDown { get; set; }

        protected int LastCaretPos { get; set; }

        protected int LastCaretSize { get; set; }
    }
}
