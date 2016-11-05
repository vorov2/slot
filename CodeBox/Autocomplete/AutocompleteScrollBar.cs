using CodeBox.Autocomplete;
using CodeBox.Styling;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Autocomplete
{
    public sealed class AutocompleteScrollBar
    {
        private readonly Editor editor;
        private readonly AutocompleteWindow window;
        private bool mouseDown;
        private int lastCaretPos;
        private int lastCaretSize;

        public AutocompleteScrollBar(Editor editor, AutocompleteWindow window)
        {
            this.editor = editor;
            this.window = window;
        }

        public void MouseDown(Point loc)
        {
            if (IsCaretInLocation(loc))
                mouseDown = true;
            else
            {
                var value = GetScrollValue(loc);
                window.SetScrollPositionY(value);
            }
        }

        public void MouseUp(Point loc) => mouseDown = false;

        public void MouseMove(Point loc)
        {
            if (mouseDown)
            {
                var value = GetScrollValue(loc);
                window.SetScrollPositionY(value);
            }
        }

        private int GetScrollValue(Point loc)
        {
            long max = window.ScrollMax;
            var v = loc.Y;
            var ret = -(max * (v - lastCaretSize / 2) / (window.Height - lastCaretSize));

            if (ret > 0)
                ret = 0;
            else if (ret < -max)
                ret = -max;

            return (int)ret;
        }

        public void Draw(Graphics g, Rectangle bounds)
        {
            if (Size <= 0)
                return;

            var scY = window.ScrollPosition;
            var scHeight = window.ScrollMax;
            g.FillRectangle(editor.Styles.Default.BackBrush, bounds);

            var caretSize = ((double)bounds.Height / (bounds.Height + scHeight)) * bounds.Height;

            if (caretSize < editor.Info.LineHeight)
                caretSize = editor.Info.LineHeight;

            var perc = (bounds.Height - caretSize) / 100d;
            var curpos = scHeight != 0 ? (int)Math.Floor(Math.Floor(scY / (scHeight / 100d)) * perc) : 0;

            lastCaretSize = (int)Math.Floor(caretSize);
            var pos = bounds.Y + Math.Abs(curpos);

            if (pos + lastCaretSize > window.ClientSize.Height)
                pos = window.ClientSize.Height - lastCaretSize;

            g.FillRectangle(editor.CachedBrush.Create(
                mouseDown ? editor.Settings.ScrollActiveThumbColor : editor.Settings.ScrollThumbColor),
                new Rectangle(bounds.X, pos, bounds.Width, lastCaretSize));
            lastCaretPos = pos;
        }

        private bool IsCaretInLocation(Point loc) =>
            loc.Y >= lastCaretPos && loc.Y < lastCaretPos + lastCaretSize;

        public int Size { get; set; }
    }
}
