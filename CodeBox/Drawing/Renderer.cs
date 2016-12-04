using CodeBox.Folding;
using CodeBox.Margins;
using CodeBox.ObjectModel;
using CodeBox.StatusBar;
using CodeBox.Styling;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeBox.Drawing
{
    internal sealed class Renderer
    {
        private readonly Editor editor;
        private static readonly StringFormat format = new StringFormat(StringFormat.GenericTypographic)
        {
            LineAlignment = StringAlignment.Center,
            Alignment = StringAlignment.Center,
            Trimming = StringTrimming.None
        };

        public Renderer(Editor editor)
        {
            this.editor = editor;
        }

        internal int DrawLineLengthIndicator(Graphics g, int len, int x, int y)
        {
            if (x + editor.Scroll.ScrollPosition.X < editor.Info.TextLeft)
                return 0;

            var shift = editor.ShowEol ? editor.Info.CharWidth / 2 : 0;
            x += shift;
            var str = len.ToString();
            g.DrawString(str, editor.Settings.SmallFont, editor.Theme.GetStyle(StandardStyle.SpecialSymbol).ForeColor.Brush(),
                new PointF(x, y), TextStyle.Format);
            return shift + (str.Length + 1) * editor.Info.CharWidth;
        }

        internal void DrawLongLineIndicators(Graphics g)
        {
            if (editor.WordWrap)
                return;

            foreach (var i in editor.Settings.LongLineIndicators)
            {
                var x = editor.Info.TextLeft + i * editor.Info.CharWidth
                    + editor.Scroll.ScrollPosition.X;

                if (x <= editor.Info.TextLeft)
                    continue;

                g.DrawLine(editor.Theme.GetStyle(StandardStyle.SpecialSymbol).ForeColor.Pen(),
                    x, editor.Info.TextTop, x, editor.Info.TextBottom);
            }
        }

        internal void DrawWordWrapColumn(Graphics g)
        {
            if (!editor.WordWrap || editor.WordWrapColumn == 0)
                return;

            var x = editor.Info.TextLeft + editor.WordWrapColumn 
                * editor.Info.CharWidth + editor.Scroll.ScrollPosition.X;

            if (x > editor.Info.TextLeft)
                g.DrawLine(editor.Theme.GetStyle(StandardStyle.SpecialSymbol).ForeColor.Pen(),
                    x, editor.Info.TextTop, x, editor.Info.TextBottom);
        }

        internal int DrawFoldingIndicator(Graphics g, int x, int y)
        {
            var fs = (MarginStyle)editor.Theme.GetStyle(StandardStyle.Folding);
            var w = editor.Info.CharWidth * 3;
            g.FillRectangle(fs.ActiveForeColor.Brush(),
                new Rectangle(x, y + editor.Info.LineHeight / 4, w, editor.Info.LineHeight / 2));
            g.DrawString("···", editor.Settings.Font.Get(editor.Theme.DefaultStyle.FontStyle),
                fs.BackColor.Brush(),
                new Point(x, y), TextStyle.Format);
            return w;
        }

        internal bool DrawCurrentLineIndicator(Graphics g, int y)
        {
            if (!editor.CurrentLineIndicator ||
                (!editor.Buffer.Selections.Main.IsEmpty && editor.Buffer.Selections.Main.Start.Line != editor.Buffer.Selections.Main.End.Line))
                return false;

            var ccs = editor.Theme.GetStyle(StandardStyle.CurrentLine);
            g.FillRectangle(ccs.BackColor.Brush(),
                new Rectangle(editor.Info.TextLeft - editor.Scroll.ScrollPosition.X,
                    y, editor.Info.TextWidth, editor.Info.LineHeight));
            return true;
        }

        internal void DrawLines(Graphics g, List<CaretData> carets)
        {
            CurrentFont = editor.Settings.Font;
            var fvl = editor.Scroll.FirstVisibleLine;
            var lvl = editor.Scroll.LastVisibleLine;

            for (var i = fvl; i < lvl + 1; i++)
            {
                var ln = editor.Buffer.Document.Lines[i];
                if (!ln.Folding.Has(FoldingStates.Invisible))
                    DrawLine(g, ln, i, carets);
            }
        }

        private void DrawLine(Graphics g, Line line, int lineIndex, List<CaretData> carets)
        {
            var lmarg = editor.Info.TextLeft;
            var tmarg = editor.Info.TextTop;
            var cwidth = editor.Info.TextRight;
            var x = lmarg;
            var y = tmarg + line.Y;
            var oldcut = 0;
            var sel = editor.Buffer.Selections.IsLineSelected(lineIndex);
            var showEol = editor.ShowEol;
            var showWs = editor.ShowWhitespace;

            for (var j = 0; j < line.Stripes; j++)
            {
                var curline = false;

                if (lineIndex == editor.Buffer.Selections.Main.Caret.Line)
                    curline = DrawCurrentLineIndicator(g, y);

                var cut = line.GetCut(j);

                if (cut == line.Length)
                    cut++;

                var tet = 0;

                for (var i = oldcut; i < cut; i++)
                {
                    var c = line.CharAt(i);
                    var ct = c == '\t' ? Line.GetIndentationSize(tet, editor.IndentSize) : 1;
                    tet += ct;
                    var xw = ct * editor.Info.CharWidth;
                    var visible = x + editor.Scroll.ScrollPosition.X >= lmarg && x + editor.Scroll.ScrollPosition.X + xw <= cwidth
                        && y + editor.Scroll.ScrollPosition.Y >= tmarg;

                    if (visible)
                    {
                        var style = default(TextStyle);
                        var rect = new Rectangle(x, y, xw, editor.Info.LineHeight);
                        var pos = new Pos(lineIndex, i);
                        var high = sel && editor.Buffer.Selections.IsSelected(pos);

                        if (c == '\0' && showEol || (c == '\t' || c == ' ') && showWs)
                        {
                            c = c == '\0' ? '\u00B6' : c == '\t' ? '\u2192' : '·';
                            style = (TextStyle)editor.Theme.GetStyle(StandardStyle.SpecialSymbol);
                            style = style.Combine(line.GetStyle(i, editor.Theme));
                        }
                        else
                            style = line.GetStyle(i, editor.Theme);

                        if (high)
                        {
                            var sstyle = (TextStyle)editor.Theme.GetStyle(StandardStyle.Selection);
                            style = sstyle.Combine(style);
                        }

                        style.DrawAll(g, rect, c, pos);

                        if (editor.Buffer.Selections.HasCaret(pos))
                        {
                            var blink = editor.Buffer.Selections.Main.Caret.Line == lineIndex
                                && editor.Buffer.Selections.Main.Caret.Col == i;

                            if (blink)
                            {
                                var cg = editor.CaretRenderer.GetDrawingSurface();
                                cg.Clear(high ? editor.Theme.GetStyle(StandardStyle.Selection).BackColor
                                    : curline ? editor.Theme.GetStyle(StandardStyle.CurrentLine).BackColor
                                    : editor.Theme.DefaultStyle.BackColor);
                                style.DrawAll(cg, new Rectangle(default(Point), rect.Size), c, pos);

                                if (editor.Settings.LongLineIndicators.Any(ind => ind == i) || (editor.WordWrap && editor.WordWrapColumn == i))
                                    cg.DrawLine(editor.Theme.GetStyle(StandardStyle.SpecialSymbol).ForeColor.Pen(), 0, 0, 0, rect.Size.Height);

                                editor.CaretRenderer.Resume();
                            }

                            carets.Add(new CaretData(x, y, pos.Line, pos.Col, blink));
                        }
                    }

                    x += xw;
                }

                var addedWidth = 0;

                if (line.Length > 0 && editor.ShowLineLength)
                    addedWidth = DrawLineLengthIndicator(g, line.Length, x, y);

                if (line.Folding.Has(FoldingStates.Header) && lineIndex + 1 < editor.Buffer.Document.Lines.Count &&
                    editor.Buffer.Document.Lines[lineIndex + 1].Folding.Has(FoldingStates.Invisible))
                    DrawFoldingIndicator(g, x + addedWidth, y);

                oldcut = cut;
                y += editor.Info.LineHeight;
                x = lmarg;
            }
        }

        internal void DrawMargins(int start, Graphics g, MarginList margins)
        {
            var vertical = margins == editor.LeftMargins || margins == editor.RightMargins;
            var top = margins == editor.TopMargins;

            foreach (var m in margins)
            {
                var bounds = default(Rectangle);

                if (vertical)
                    bounds = new Rectangle(start, editor.Info.TextTop, m.CalculateSize(),
                        editor.ClientSize.Height - editor.Info.TextTop); //HACK
                else if (top)
                    bounds = new Rectangle(0, start, editor.ClientSize.Width, m.CalculateSize());
                else
                    bounds = new Rectangle(editor.Info.TextLeft, start, editor.Info.TextWidth, m.CalculateSize());

                m.Draw(g, bounds);
                start += m.CalculateSize();
            }
        }

        internal static Font CurrentFont { get; private set; }
    }
}
