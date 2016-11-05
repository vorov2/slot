using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Drawing
{
    internal sealed class CachedBrush : IDisposable
    {
        private readonly Dictionary<Color, Brush> cache = new Dictionary<Color, Brush>();

        public void Dispose() => Reset();

        public void Reset()
        {
            foreach (var b in cache.Values)
                b.Dispose();
            cache.Clear();
        }

        public void ResetBrush(Color color)
        {
            Brush b;

            if (cache.TryGetValue(color, out b))
            {
                cache.Remove(color);
                b.Dispose();
            }
        }

        public Brush Create(Color color)
        {
            Brush b;

            if (!cache.TryGetValue(color, out b))
                cache.Add(color, b = new SolidBrush(color));

            return b;
        }
    }
}
