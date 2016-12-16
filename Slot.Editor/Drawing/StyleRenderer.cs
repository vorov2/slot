using System;
using System.Drawing;
using Slot.Core.Themes;
using Slot.Drawing;
using Slot.Editor.ObjectModel;

namespace Slot.Editor.Drawing
{
    internal static class StyleRenderer
    {
        internal static void DrawAll(Style style, Graphics g, Rectangle rect, char ch, Pos pos)
        {
            DrawBackground(style, g, rect, pos);
            DrawText(style, g, rect, ch, pos);
            DrawAdornment(style, g, rect, pos);
        }

        public static void DrawText(Style style, Graphics g, Rectangle rect, char ch, Pos pos)
        {
            var fc = style.ForeColor.IsEmpty ? DefaultStyle.ForeColor : style.ForeColor;
            g.DrawString(ch.ToString(),
                Renderer.CurrentFont.Get(style.FontStyle),
                fc.Brush(),
                rect.Location, TextFormats.Compact);
        }

        public static void DrawAdornment(Style style, Graphics g, Rectangle rect, Pos pos)
        {
            if (style.Adornment == Adornment.Line)
            {
                var p = style.AdornmentColor.ThickPen();
                var pw = (int)Math.Round(p.Width, MidpointRounding.AwayFromZero);
                g.DrawLine(p,
                    new Point(rect.X, rect.Y + rect.Height - pw),
                    new Point(rect.X + rect.Width, rect.Y + rect.Height - pw));
            }
        }

        public static void DrawBackground(Style style, Graphics g, Rectangle rect, Pos pos)
        {
            if (!style.BackColor.IsEmpty && style.BackColor != DefaultStyle.BackColor)
                g.FillRectangle(style.BackColor.Brush(), rect);
        }

        internal static Style DefaultStyle { get; set; }
    }
}
