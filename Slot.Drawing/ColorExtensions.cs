using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Slot.Drawing
{
    public static class ColorExtensions
    {
        private static readonly Dictionary<Color, Brush> brushCache = new Dictionary<Color, Brush>();
        private static readonly Dictionary<Color, Pen> penCache = new Dictionary<Color, Pen>();
        private static readonly Dictionary<Color, Pen> dashedPenCache = new Dictionary<Color, Pen>();
        private static readonly Dictionary<Color, Pen> dottedPenCache = new Dictionary<Color, Pen>();
        private static readonly Dictionary<Color, Pen> thickPenCache = new Dictionary<Color, Pen>();

        public static void Clean()
        {
            Clean(brushCache);
            Clean(penCache);
            Clean(dashedPenCache);
            Clean(dottedPenCache);
        }

        private static void Clean<T>(Dictionary<Color,T> cache) where T : IDisposable
        {
            foreach (var b in cache.Values)
                b.Dispose();
            cache.Clear();
        }

        public static Brush Brush(this Color color)
        {
            Brush b;

            if (!brushCache.TryGetValue(color, out b))
                brushCache.Add(color, b = new SolidBrush(color));

            return b;
        }

        public static Pen Pen(this Color color)
        {
            Pen p;

            if (!penCache.TryGetValue(color, out p))
            {
                p = new Pen(color);
                p.Alignment = PenAlignment.Inset;
                p.Width = Dpi.GetWidth(1);
                penCache.Add(color, p);
            }

            return p;
        }

        public static Pen ThickPen(this Color color)
        {
            Pen p;

            if (!thickPenCache.TryGetValue(color, out p))
            {
                p = new Pen(color);
                p.Width = Dpi.GetWidth(2);
                p.Alignment = PenAlignment.Inset;
                thickPenCache.Add(color, p);
            }

            return p;
        }

        public static Pen DashedPen(this Color color)
        {
            Pen p;

            if (!dashedPenCache.TryGetValue(color, out p))
            {
                p = new Pen(color);
                p.Alignment = PenAlignment.Inset;
                p.Width = Dpi.GetWidth(1);
                p.DashStyle = DashStyle.Dash;
                dashedPenCache.Add(color, p);
            }

            return p;
        }

        public static Pen DottedPen(this Color color)
        {
            Pen p;

            if (!dottedPenCache.TryGetValue(color, out p))
            {
                p = new Pen(color);
                p.Alignment = PenAlignment.Inset;
                p.Width = Dpi.GetWidth(1);
                p.DashStyle = DashStyle.Dot;
                dottedPenCache.Add(color, p);
            }

            return p;
        }
    }

    public static class PenExtensions
    {
        public static int Size(this Pen pen) => pen.Width == 1 ? (int)pen.Width : 0;
    }
}
