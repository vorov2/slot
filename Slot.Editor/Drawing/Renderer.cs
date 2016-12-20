using Slot.Core.Themes;
using Slot.Editor.Folding;
using Slot.Editor.Margins;
using Slot.Editor.ObjectModel;
using Slot.Editor.Styling;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Slot.Drawing;

namespace Slot.Editor.Drawing
{
    internal sealed class Renderer
    {
        private readonly EditorControl editor;

        public Renderer(EditorControl editor)
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
                new PointF(x, y), TextFormats.Compact);
            return shift + (str.Length + 1) * editor.Info.CharWidth;
        }

        internal void DrawLongLineIndicators(Graphics g)
        {
            if (editor.WordWrap || editor.LimitedMode)
                return;

            foreach (var i in editor.Settings.LongLineIndicators)
            {
                var x = editor.Info.TextLeft + i * editor.Info.CharWidth
                    + editor.Scroll.ScrollPosition.X;

                if (x <= editor.Info.TextLeft)
                    continue;

                g.DrawLine(editor.Theme.GetStyle(StandardStyle.SpecialSymbol).ForeColor.Pen(),
                    x, editor.Info.TextTop, x, editor.ClientSize.Height);// editor.Info.TextBottom);
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
            var fs = editor.Theme.GetStyle(StandardStyle.Folding);
            var w = editor.Info.CharWidth * 3;
            g.FillRectangle(editor.Theme.GetStyle(StandardStyle.ActiveFolding).ForeColor.Brush(),
                new Rectangle(x, y + editor.Info.LineHeight / 4, w, editor.Info.LineHeight / 2));
            g.DrawString("···", editor.Settings.Font.Get(editor.Theme.GetStyle(StandardStyle.Default).FontStyle),
                fs.BackColor.Brush(),
                new Point(x, y), TextFormats.Compact);
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
                    y, editor.ClientSize.Width - editor.Info.TextLeft, editor.Info.LineHeight));
            return true;
        }

        internal void DrawLines(Graphics g, List<CaretData> carets)
        {
            CurrentFont = editor.Settings.Font;
            var fvl = editor.Scroll.FirstVisibleLine;
            var lvl = editor.Scroll.LastVisibleLine;
            StyleRenderer.DefaultStyle = editor.Theme.GetStyle(StandardStyle.Default);

            for (var i = fvl; i < lvl + 1; i++)
            {
                if (editor.Folding.IsLineVisible(i))
                    DrawLine(g, editor.Buffer.Document.Lines[i], i, carets);
            }
        }

        private void DrawLine(Graphics g, Line line, int lineIndex, List<CaretData> carets)
        {
            var lmarg = editor.Info.TextLeft;
            var tmarg = editor.Info.TextTop;
            var cwidth = editor.ClientSize.Width;//editor.Info.TextRight;
            var x = lmarg;
            var y = tmarg + line.Y;
            var oldcut = 0;
            var sel = editor.Buffer.Selections.IsLineSelected(lineIndex);
            var showEol = editor.ShowEol;
            var showWs = editor.ShowWhitespace;
            var specialSymbol = editor.Theme.GetStyle(StandardStyle.SpecialSymbol);
            var selection = editor.Theme.GetStyle(StandardStyle.Selection);
            var currentLine = editor.Theme.GetStyle(StandardStyle.CurrentLine);
            var indent = -1;
            var nonWs = false;
            var showInd = editor.Settings.ShowIndentationGuides;
            var indentSize = editor.IndentSize;

            for (var j = 0; j < line.Stripes; j++)
            {
                var cut = line.GetCut(j);

                if (cut == line.Length)
                    cut++;

                if (y + editor.Scroll.ScrollPosition.Y >= tmarg && y + editor.Scroll.ScrollPosition.Y < editor.ClientSize.Height/*editor.Info.TextBottom*/)
                {
                    var curline = false;

                    if (lineIndex == editor.Buffer.Selections.Main.Caret.Line)
                        curline = DrawCurrentLineIndicator(g, y);

                    var tet = 0;

                    if (indent != -1)
                    {
                        tet = indent;
                        x += tet * editor.Info.CharWidth;
                    }

                    for (var i = oldcut; i < cut; i++)
                    {
                        var c = line.CharAt(i);
                        var ct = c == '\t' ? Line.GetIndentationSize(tet, editor.IndentSize) : Line.GetCharWidth(c);

                        tet += ct;
                        var xw = ct * editor.Info.CharWidth;
                        var visible = x + editor.Scroll.ScrollPosition.X >= lmarg && x + editor.Scroll.ScrollPosition.X + xw <= cwidth;
                        var guide = false;
                        
                        if (visible)
                        {
                            var style = Style.Empty;
                            var rect = new Rectangle(x, y, xw, editor.Info.LineHeight);
                            var pos = new Pos(lineIndex, i);
                            var high = sel && editor.Buffer.Selections.IsSelected(pos);
                            var ws = c == '\t' || c == ' ';

                            if (!ws && !nonWs)
                                nonWs = true;

                            if (!nonWs && showInd && (tet - ct) % indentSize == 0)
                            {
                                DrawIndentationGuide(g, specialSymbol, x, y);
                                guide = true;
                            }

                            if (c == '\0' && showEol || ws && (showWs == ShowWhitespace.All
                                || showWs == ShowWhitespace.Boundary && !nonWs
                                || showWs == ShowWhitespace.Selection && high))
                            {
                                c = c == '\0' ? '\u00B6' : c == '\t' ? '\u2192' : '·';
                                style = specialSymbol;
                                style = style.Combine(line.GetStyle(i, editor.Theme));
                            }
                            else
                                style = line.GetStyle(i, editor.Theme);

                            if (high)
                            {
                                var sstyle = selection;
                                style = sstyle.Combine(style);
                            }

                            StyleRenderer.DrawAll(style, g, rect, c, pos);

                            if (editor.Buffer.Selections.HasCaret(pos))
                            {
                                var blink = editor.Buffer.Selections.Main.Caret.Line == lineIndex
                                    && editor.Buffer.Selections.Main.Caret.Col == i;

                                if (blink)
                                {
                                    var cg = editor.CaretRenderer.GetDrawingSurface();
                                    cg.Clear(high ? selection.BackColor
                                        : curline ? currentLine.BackColor
                                        : StyleRenderer.DefaultStyle.BackColor);
                                    StyleRenderer.DrawAll(style, cg, new Rectangle(default(Point), rect.Size), c, pos);

                                    if (editor.Settings.LongLineIndicators.Any(ind => ind == i) || (editor.WordWrap && editor.WordWrapColumn == i))
                                        cg.DrawLine(specialSymbol.ForeColor.Pen(), 0, 0, 0, rect.Size.Height);

                                    if (guide)
                                        DrawIndentationGuide(cg, specialSymbol, 0, 0);

                                    if (editor.Scroll.ScrollPosition.X != 0 && x + editor.Scroll.ScrollPosition.X == editor.Info.TextLeft)
                                    {
                                        cg.FillRectangle(ControlPaint.Dark(StyleRenderer.DefaultStyle.BackColor, .05f).Brush(),
                                            new Rectangle(0, 0, Dpi.GetWidth(2), rect.Height));
                                    }

                                    editor.CaretRenderer.Resume();
                                }

                                carets.Add(new CaretData(x, y, pos.Line, pos.Col, blink));
                            }
                        }

                        x += xw;
                    }

                    var addedWidth = 0;

                    if (line.Length > 0 && editor.ShowLineLength && j == line.Stripes - 1)
                        addedWidth = DrawLineLengthIndicator(g, line.Length, x, y);

                    if (line.Folding.Has(FoldingStates.Header) && lineIndex + 1 < editor.Buffer.Document.Lines.Count &&
                        !editor.Folding.IsLineVisible(lineIndex + 1))
                        DrawFoldingIndicator(g, x + addedWidth, y);
                }

                oldcut = cut;
                y += editor.Info.LineHeight;
                x = lmarg;
                indent = line.Indent;
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

        internal void DrawIndentationGuide(Graphics g, Style style, int x, int y)
        {
            g.DrawLine(style.ForeColor.DottedPen(), x, y, x, y + editor.Info.LineHeight);
        }

        internal static Font CurrentFont { get; private set; }
    }
}
