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
                var line = editor.Lines[i];

                if (line.Folding.Has(FoldingStates.Invisible))
                    continue;

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

            var col = FindColumnByLocation(editor.Lines[line], loc);
            return new Pos(line, col);
        }

        public Point PositionToLocation(Pos pos)
        {
            var line = editor.Lines[pos.Line];
            var y = line.Y + line.GetStripe(pos.Col) * editor.Info.LineHeight + editor.Info.LineHeight
                + editor.Info.TextTop;
            var x = line.GetTetras(pos.Col, editor.IndentSize) * editor.Info.CharWidth
                + editor.Info.TextLeft;
            return new Point(x + editor.Scroll.ScrollPosition.X, y + editor.Scroll.ScrollPosition.Y);
        }

        private int FindColumnByLocation(Line line, Point loc)
        {
            var stripe = (int)Math.Ceiling((loc.Y - editor.Info.TextTop - line.Y - editor.Scroll.ScrollPosition.Y)
                / (double)editor.Info.LineHeight) - 1;
            var cut = line.GetCut(stripe);
            var sc = stripe > 0 ? line.GetCut(stripe - 1) + 1 : 0;
            var width = editor.Info.TextLeft;
            var locX = loc.X - editor.Scroll.ScrollPosition.X;
            var app = editor.Info.CharWidth * .15;

            for (var i = sc; i < cut + 1; i++)
            {
                var c = line.CharAt(i);
                var cw = c == '\t' ? editor.IndentSize * editor.Info.CharWidth : editor.Info.CharWidth;

                if (locX >= width - app && locX <= width + cw - app)
                    return i;

                width += cw;
            }

            return locX > width - editor.Info.CharWidth ? line.Length : 0;
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
