using CodeBox.Folding;
using CodeBox.Margins;
using CodeBox.ObjectModel;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace CodeBox
{
    public sealed class LocationManager
    {
        private readonly Editor editor;

        internal LocationManager(Editor editor)
        {
            this.editor = editor;
        }

        public int FindLineByLocation(int locY)
        {
            locY = locY - editor.Scroll.ScrollPosition.Y;

            for (var i = editor.Scroll.FirstVisibleLine; i < editor.Scroll.LastVisibleLine + 1; i++)
            {
                if (!editor.Folding.IsLineVisible(i))
                    continue;

                var line = editor.Lines[i];
                var lh = line.Stripes * editor.Info.LineHeight;
                var lineY = line.Y + editor.Info.TextTop;

                if (locY >= lineY && locY <= lineY + lh)
                    return i;
            }

            return locY + editor.Scroll.ScrollPosition.Y < editor.Info.TextTop ? 0 : editor.Lines.Count - 1;
        }

        public Pos LocationToPosition(Point loc)
        {
            var line = FindLineByLocation(loc.Y);

            if (line == -1)
                return Pos.Empty;

            var ln = editor.Lines[line];
            var col = FindColumnByLocation(editor.Lines[line], loc);
            var stripe = ln.GetStripe(col);

            if (stripe > 0)
            {
                
            }

            return new Pos(line, col);
        }

        public Point PositionToLocation(Pos pos)
        {
            var line = editor.Lines[pos.Line];
            var stripe = line.GetStripe(pos.Col);
            var y = line.Y + stripe * editor.Info.LineHeight + editor.Info.LineHeight
                + editor.Info.TextTop;
            
            var tetras = line.GetTetras(pos.Col, editor.IndentSize);

            if (stripe > 0)
                tetras -= line.GetTetras(line.GetCut(stripe - 1), editor.IndentSize) - line.Indent;

            var x = tetras * editor.Info.CharWidth + editor.Info.TextLeft;
            return new Point(x + editor.Scroll.ScrollPosition.X, y + editor.Scroll.ScrollPosition.Y);
        }

        private int FindColumnByLocation(Line line, Point loc)
        {
            var stripe = (int)Math.Ceiling((loc.Y - editor.Info.TextTop - line.Y - editor.Scroll.ScrollPosition.Y)
                / (double)editor.Info.LineHeight) - 1;
            var cut = line.GetCut(stripe < 0 ? 0 : stripe);
            var sc = stripe > 0 ? line.GetCut(stripe - 1) : 0;
            var width = editor.Info.TextLeft;
            var locX = loc.X - editor.Scroll.ScrollPosition.X;
            var app = editor.Info.CharWidth * .50;

            if (stripe > 0)
            {
                locX -= line.Indent * editor.Info.CharWidth;
                if (locX < width)
                    locX = width;
            }

            for (var i = sc; i < cut + 1; i++)
            {
                var c = line.CharAt(i);
                var cw = c == '\t' ? editor.IndentSize * editor.Info.CharWidth
                    : Line.GetCharWidth(c) * editor.Info.CharWidth;

                if (locX >= width - app && locX <= width + cw - app)
                    return i;

                width += cw;
            }

            return locX > width - editor.Info.CharWidth ? cut : 0;
        }

        public MarginList FindMargin(Point loc)
        {
            if (loc.X < editor.Info.TextLeft)
                return editor.LeftMargins;
            else if (loc.X > editor.Info.TextRight)
                return editor.RightMargins;
            else if (loc.Y < editor.Info.TextTop)
                return editor.TopMargins;
            else if (loc.Y > editor.Info.TextBottom)
                return editor.BottomMargins;
            else
                return null;
        }
    }
}
