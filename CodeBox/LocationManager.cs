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
            int y;
            return FindLineByLocation(locY, out y);
        }

        private int FindLineByLocation(int locY, out int lineY)
        {
            lineY = editor.Info.TextTop;

            for (var i = editor.Scroll.FirstVisibleLine; i < editor.Scroll.LastVisibleLine + 1; i++)
            {
                var line = editor.Lines[i];
                var lh = line.Stripes * editor.Info.LineHeight;

                if (locY >= lineY && locY <= lineY + lh)
                    return i;

                lineY += lh;
            }

            return -1;
        }

        public Pos LocationToPosition(Point loc)
        {
            int lineY;
            var line = FindLineByLocation(loc.Y, out lineY);

            if (line == -1)
                return Pos.Empty;

            var col = FindColumnByLocation(editor.Lines[line], lineY, loc);
            return new Pos(line, col);
        }

        private int FindColumnByLocation(Line line, int lineY, Point loc)
        {
            var stripe = (int)Math.Ceiling((loc.Y - lineY) / (double)editor.Info.LineHeight) - 1;
            var cut = line.GetCut(stripe);
            var sc = stripe > 0 ? line.GetCut(stripe - 1) + 1 : 0;
            var width = editor.Info.TextLeft;
            var locX = loc.X - editor.Scroll.X;
            var app = editor.Info.CharWidth * .15;

            for (var i = sc; i < cut + 1; i++)
            {
                var c = line.CharAt(i);
                var cw = c == '\t' ? editor.Settings.TabSize * editor.Info.CharWidth : editor.Info.CharWidth;

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
