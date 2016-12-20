using Slot.Core.Themes;
using Slot.Drawing;
using Slot.Editor.Search;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Slot.Editor.Margins
{
    public sealed class VerticalScrollBarMargin : ScrollBarMargin
    {
        public VerticalScrollBarMargin(EditorControl editor) : base(editor, Orientation.Vertical)
        {
        }

        protected override bool OnDraw(Graphics g, Rectangle bounds)
        {
            var sc = new Rectangle(Editor.Scroll.ScrollPosition, Editor.Scroll.ScrollBounds);
            var sbs = Editor.Theme.GetStyle(StandardStyle.ScrollBars);
            var asbs = Editor.Theme.GetStyle(StandardStyle.ActiveScrollBar);
            //g.FillRectangle(sbs.BackColor.Brush(), bounds);

            var caretSize = ((double)bounds.Height / (bounds.Height + sc.Height)) * bounds.Height;

            if (caretSize < Editor.Info.LineHeight)
                caretSize = Editor.Info.LineHeight;

            var perc = (bounds.Height - caretSize) / 100d;
            var curpos = sc.Height != 0 ? (int)Math.Floor(Math.Floor(sc.Y / (sc.Height / 100d)) * perc) : 0;

            LastCaretSize = (int)Math.Floor(caretSize);
            var pos = bounds.Y + Math.Abs(curpos);

            if (pos + LastCaretSize > Editor.ClientSize.Height)
                pos = Editor.ClientSize.Height - LastCaretSize;

            if (LastCaretSize != bounds.Height)
                g.FillRectangle((IsMouseDown ? asbs.ForeColor : sbs.ForeColor).Brush(),
                    new Rectangle(bounds.X, pos, bounds.Width, LastCaretSize));

            LastCaretPos = pos;
            var caretLine = Editor.Buffer.Selections.Main.Caret.Line;

            foreach (var s in Editor.Buffer.Selections)
            {
                var linePos = s.Caret.Line / (Editor.Lines.Count / 100d);
                var caretY = Editor.Info.TextTop + linePos * (bounds.Height / 100d);

                g.FillRectangle(Editor.Theme.GetStyle(StandardStyle.Default).ForeColor.Brush(),
                    new Rectangle(bounds.X, (int)caretY, bounds.Width,
                        (int)Math.Round(g.DpiY / 96f) * s.Caret.Line == caretLine ? 2 : 1));
            }

            if (Editor.Search.HasSearchResults)
            {
                var hl = Editor.Theme.GetStyle(StandardStyle.SearchItem);
                MarkOccurences(g, hl, bounds, Editor.Search.EnumerateSearchResults());
            }
            else if (Editor.MatchWords.HasSearchResults)
            {
                var hl = Editor.Theme.GetStyle(StandardStyle.MatchedWord);
                MarkOccurences(g, hl, bounds, Editor.MatchWords.EnumerateSearchResults());
            }

            return true;
        }

        private void MarkOccurences(Graphics g, Style hl, Rectangle bounds, IEnumerable<SearchResult> seq)
        {
            var markHeight = bounds.Height / Editor.Lines.Count;
            var min = Dpi.GetHeight(6);
            markHeight = markHeight < min ? min : markHeight;
            var lastLine = -1;

            foreach (var f in seq)
            {
                if (f.Line == lastLine)
                    continue;

                var linePos = f.Line / (Editor.Lines.Count / 100f);
                var markY = Editor.Info.TextTop + linePos * (bounds.Height / 100f);
                var w = Dpi.GetWidth(5);

                g.FillRectangle((hl.AdornmentColor.IsEmpty ? hl.BackColor : hl.AdornmentColor).Brush(), new RectangleF(
                    bounds.X + (bounds.Width - w) / 2f,
                    markY,
                    w,
                    Dpi.GetHeight(markHeight)));
                lastLine = f.Line;
            }
        }

        public override int CalculateSize() => (int)(Editor.Info.CharWidth * 1.5);
    }
}
