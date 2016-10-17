using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox
{
    internal sealed class Renderer : IDisposable
    {
        private readonly Dictionary<Color, Brush> brushCache = new Dictionary<Color, Brush>();
        private readonly Dictionary<FontInfo, Font> fontCache = new Dictionary<FontInfo, Font>();

        public void Dispose()
        {
            Reset();
        }

        public void Reset()
        {
            foreach (var b in brushCache.Values)
                b.Dispose();
            brushCache.Clear();

            foreach (var f in fontCache.Values)
                f.Dispose();
            fontCache.Clear();
        }

        public Brush GetBrush(Color color)
        {
            Brush b;

            if (!brushCache.TryGetValue(color, out b))
                brushCache.Add(color, b = new SolidBrush(color));

            return b;
        }

        public Font GetFont(string name, float size, FontStyle style)
        {
            Font f;
            var fi = new FontInfo(name, size, style);

            if (!fontCache.TryGetValue(fi, out f))
                fontCache.Add(fi, f = new Font(name, size, style));

            return f;
        }
    }

    internal sealed class FontInfo
    {
        public FontInfo(string name, float size, FontStyle style)
        {
            Name = (name ?? "").ToLower();
            Size = size;
            Style = style;
        }

        public string Name { get; }

        public float Size { get; }

        public FontStyle Style { get; }

        public override bool Equals(object obj)
        {
            if (!(obj is FontInfo))
                return false;

            var fi = (FontInfo)obj;
            return string.Equals(fi.Name, Name, StringComparison.OrdinalIgnoreCase)
                && fi.Size == Size && fi.Style == Style;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;

                if (Name != null)
                    hash = hash * 23 + Name.GetHashCode();

                hash = hash * 23 + Size.GetHashCode();
                hash = hash * 23 + Style.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            return $"{Name} ({Size}pt;{Style})";
        }
    }
}
