using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Styling
{
    public sealed class StyleInfo
    {
        public StyleInfo(Color fore) : this(fore, null, null)
        {

        }

        public StyleInfo(Color fore, Color back) : this(fore, back, null)
        {

        }

        public StyleInfo(Color fore, Color back, FontStyle style) : this((Color?)fore, back, style)
        {

        }

        public StyleInfo(Color? fore, Color? back, FontStyle? style)
        {
            ForeColor = fore;
            BackColor = back;
            FontStyle = style;
        }

        public Color? ForeColor { get; }

        public Color? BackColor { get; }

        public FontStyle? FontStyle { get; }
    }
}
