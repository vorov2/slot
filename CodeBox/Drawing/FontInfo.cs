using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Drawing
{
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
