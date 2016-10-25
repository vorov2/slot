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
            var line = editor.Lines[editor.Scroll.FirstVisibleLine];
            var lineIndex = 0;
            var y = editor.Info.TextTop;
            locY = locY - editor.Scroll.Y;

            do
            {
                var lh = line.Stripes * editor.Info.LineHeight;

                if (locY >= y && locY <= y + lh)
                    return lineIndex;

                if (editor.Lines.Count == lineIndex + 1)
                    return -1;

                y += lh;
                line = editor.Lines[++lineIndex];
            } while (true);
        }

        public int FindColumnByLocation(Line line, Point loc)
        {
            var stripe = (int)Math.Ceiling((loc.Y - editor.Info.TextTop - line.Y - editor.Scroll.Y)
                / (double)editor.Info.LineHeight) - 1;
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
            else if (loc.Y > editor.Info.TextBotom)
                return editor.BottomMargins;
            else
                return null;
        }
    }
}
