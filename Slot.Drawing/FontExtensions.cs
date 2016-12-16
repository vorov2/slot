using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Slot.Drawing
{
    public static class FontExtensions
    {
        private static readonly Dictionary<Font, Dictionary<FontStyle, Font>> cache = new Dictionary<Font, Dictionary<FontStyle, Font>>();
        private static readonly Dictionary<Font, Size> sizeCache = new Dictionary<Font, Size>();

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
            sizeCache.Clear();
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

        public static int Width(this Font font) => Size(font).Width;

        public static int Height(this Font font) => Size(font).Height;

        public static Size Size(this Font font)
        {
            Size ret;

            if (!sizeCache.TryGetValue(font, out ret))
            {
                using (var ctl = new Control())
                using (var g = ctl.CreateGraphics())
                {
                    var size1 = g.MeasureString("<M>", font);
                    var size2 = g.MeasureString("<>", font);
                    ret = new Size((int)(size1.Width - size2.Width), (int)font.GetHeight(g));
                }

                sizeCache.Add(font, ret);
            }

            return ret;
        }
    }
}
