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
    public sealed class VerticalScrollBarMargin : ScrollBarMargin
    {
        public VerticalScrollBarMargin(Editor editor) : base(editor, Orientation.Vertical)
        {
        }

        protected override bool OnDraw(Graphics g, Rectangle bounds)
        {
            Enabled = CalculateSize() != 0;

            if (!Enabled)
                return false;

            var sc = new Rectangle(Editor.Scroll.ScrollPosition, Editor.Scroll.ScrollBounds);
            var sbs = (MarginStyle)Editor.Theme.GetStyle(StandardStyle.ScrollBars);
            g.FillRectangle(sbs.BackColor.Brush(), bounds);

            var caretSize = ((double)bounds.Height / (bounds.Height + sc.Height)) * bounds.Height;

            if (caretSize < Editor.Info.LineHeight)
                caretSize = Editor.Info.LineHeight;

            var perc = (bounds.Height - caretSize) / 100d;
            var curpos = sc.Height != 0 ? (int)Math.Floor(Math.Floor(sc.Y / (sc.Height / 100d)) * perc) : 0;

            LastCaretSize = (int)Math.Floor(caretSize);
            var pos = bounds.Y + Math.Abs(curpos);

            if (pos + LastCaretSize > Editor.ClientSize.Height)
                pos = Editor.ClientSize.Height - LastCaretSize;

            g.FillRectangle((IsMouseDown ? sbs.ActiveForeColor : sbs.ForeColor).Brush(),
                new Rectangle(bounds.X, pos, bounds.Width, LastCaretSize));
            LastCaretPos = pos;
            var caretLine = Editor.Buffer.Selections.Main.Caret.Line;

            foreach (var s in Editor.Buffer.Selections)
            {
                var linePos = s.Caret.Line / (Editor.Lines.Count / 100d);
                var caretY = Editor.Info.TextTop + linePos * (bounds.Height / 100d);

                g.FillRectangle(Editor.Theme.DefaultStyle.ForeColor.Brush(), new Rectangle(bounds.X, (int)caretY, bounds.Width,
                    (int)Math.Round(g.DpiY / 96f) * s.Caret.Line == caretLine ? 2 : 1));
            }

            if (Editor.Search.HasSearchResults)
            {
                var hl = Editor.Theme.GetStyle(StandardStyle.SearchItem);
                var markHeight = (int)(bounds.Height / Editor.Lines.Count);
                markHeight = markHeight < 2 ? 2 : markHeight;
                var lastLine = -1;

                foreach (var f in Editor.Search.EnumerateSearchResults())
                {
                    if (f.Line == lastLine)
                        continue;

                    var linePos = f.Line / (Editor.Lines.Count / 100d);
                    var markY = Editor.Info.TextTop + linePos * (bounds.Height / 100d);
                    var w = Dpi.GetWidth(4);

                    g.FillRectangle(hl.LineColor.Brush(), new Rectangle(
                        bounds.X + (bounds.Width - w) / 2,
                        (int)markY,
                        w,
                        Dpi.GetHeight(markHeight)));
                    lastLine = f.Line;
                }
            }

            return true;
        }

        public override int CalculateSize()
        {
            if (Editor.Scroll.ScrollBounds.Height == 0)
                return 0;
            else
                return (int)(Editor.Info.CharWidth * 1.5);
        }
    }
}
