using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Drawing
{
    internal sealed class CachedPen : IDisposable
    {
        private readonly Dictionary<Color, Pen> cache = new Dictionary<Color, Pen>();

        public void Dispose()
        {
            Reset();
        }

        public void Reset()
        {
            foreach (var b in cache.Values)
                b.Dispose();
            cache.Clear();
        }

        public void ResetPen(Color color)
        {
            Pen p;

            if (cache.TryGetValue(color, out p))
            {
                cache.Remove(color);
                p.Dispose();
            }
        }

        public Pen Create(Color color)
        {
            Pen p;

            if (!cache.TryGetValue(color, out p))
                cache.Add(color, p = new Pen(color));

            return p;
        }
    }
}
