using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Drawing
{
    public static class FontExtensions
    {
        private static readonly Dictionary<Font, Dictionary<FontStyle, Font>> cache = new Dictionary<Font, Dictionary<FontStyle, Font>>();

        public static void Clean(Font baseFont)
        {
            if (baseFont == null)
                return;

            Dictionary<FontStyle, Font> fcache;

            if (!cache.TryGetValue(baseFont, out fcache))
                return;

            foreach (var f in fcache.Values)
                f.Dispose();

            fcache.Clear();
            cache.Remove(baseFont);
        }

        public static Font Get(this Font baseFont, FontStyle style)
        {
            Dictionary<FontStyle, Font> fcache;

            if (!cache.TryGetValue(baseFont, out fcache))
                cache.Add(baseFont, fcache = new Dictionary<FontStyle, Font>());

            Font f;

            if (!fcache.TryGetValue(style, out f))
                fcache.Add(style, f = new Font(baseFont, style));

            return f;
        }
    }
}
