using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Drawing
{
    internal sealed class CachedFont : IDisposable
    {
        private readonly Dictionary<FontInfo, Font> cache = new Dictionary<FontInfo, Font>();

        public void Dispose()
        {
            Reset();
        }

        public void Reset()
        {
            foreach (var f in cache.Values)
                f.Dispose();
            cache.Clear();
        }

        public Font Create(string name, float size, FontStyle style)
        {
            Font f;
            var fi = new FontInfo(name, size, style);

            if (!cache.TryGetValue(fi, out f))
                cache.Add(fi, f = new Font(name, size, style));

            return f;
        }
    }
}
