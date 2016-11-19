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

        public static void Clean()
        {
            Clean(brushCache);
            Clean(penCache);
            Clean(dashedPenCache);
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
                using (var c = new Control())
                using (var g = c.CreateGraphics())
                    p.Width = (g.DpiY / 96f);
                penCache.Add(color, p);
            }

            return p;
        }

        public static Pen CreateDashed(this Color color)
        {
            Pen p;

            if (!dashedPenCache.TryGetValue(color, out p))
            {
                p = new Pen(color);
                using (var c = new Control())
                using (var g = c.CreateGraphics())
                    p.Width = (g.DpiY / 96f);
                p.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                dashedPenCache.Add(color, p);
            }

            return p;
        }
    }
}
