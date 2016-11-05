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
        private readonly Dictionary<FontStyle, Font> cache = new Dictionary<FontStyle, Font>();
        private readonly Font baseFont;

        public CachedFont(Font baseFont)
        {
            this.baseFont = baseFont;
        }

        public void Dispose() => Reset();

        public void Reset()
        {
            foreach (var f in cache.Values)
                f.Dispose();
            cache.Clear();
        }

        public Font Create(FontStyle style)
        {
            Font f;
            
            if (!cache.TryGetValue(style, out f))
                cache.Add(style, f = new Font(baseFont, style));

            return f;
        }
    }
}
