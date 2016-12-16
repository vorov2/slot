using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace CodeBox.Drawing
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
                p.Width = Dpi.GetWidth(1);
                p.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
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
                p.Width = Dpi.GetWidth(1);
                p.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                dottedPenCache.Add(color, p);
            }

            return p;
        }
    }
}
