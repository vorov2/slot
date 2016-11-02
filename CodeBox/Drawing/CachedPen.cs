using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeBox.Drawing
{
    internal sealed class CachedPen : IDisposable
    {
        private readonly Dictionary<Color, Pen> cache = new Dictionary<Color, Pen>();
        private readonly Dictionary<Color, Pen> cacheDashed = new Dictionary<Color, Pen>();

        public void Dispose()
        {
            Reset();
        }

        public void Reset()
        {
            foreach (var b in cache.Values)
                b.Dispose();
            cache.Clear();
            foreach (var b in cacheDashed.Values)
                b.Dispose();
            cacheDashed.Clear();
        }

        public Pen Create(Color color)
        {
            Pen p;

            if (!cache.TryGetValue(color, out p))
            {
                p = new Pen(color);
                using (var c = new Control())
                using (var g = c.CreateGraphics())
                    p.Width = (g.DpiY / 96f);
                cache.Add(color, p);
            }

            return p;
        }

        public Pen CreateDashed(Color color)
        {
            Pen p;

            if (!cacheDashed.TryGetValue(color, out p))
            {
                p = new Pen(color);
                using (var c = new Control())
                using (var g = c.CreateGraphics())
                    p.Width = (g.DpiY / 96f);
                p.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                cacheDashed.Add(color, p);
            }

            return p;
        }
    }
}
