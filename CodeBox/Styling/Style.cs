using CodeBox.Drawing;
using CodeBox.ObjectModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Styling
{
    public abstract class Style
    {
        internal static readonly StringFormat Format = new StringFormat(StringFormat.GenericTypographic)
        {
            LineAlignment = StringAlignment.Near,
            Alignment = StringAlignment.Near,
            Trimming = StringTrimming.None
        };

        internal void DrawAll(Graphics g, Rectangle rect, char ch, Pos pos)
        {
            DrawBackground(g, rect, pos);
            DrawText(g, rect, ch, pos);
            DrawAdornment(g, rect, pos);
        }

        public virtual void DrawText(Graphics g, Rectangle rect, char ch, Pos pos)
        {
            var fc = ForeColor.IsEmpty && DefaultStyle != null ? DefaultStyle.ForeColor : ForeColor;
            g.DrawString(ch.ToString(),
                Renderer.CurrentFont.Get(FontStyle),
                fc.Brush(),
                rect.Location, Format);
        }

        public virtual void DrawAdornment(Graphics g, Rectangle rect, Pos pos)
        {

        }

        public virtual void DrawBackground(Graphics g, Rectangle rect, Pos pos)
        {
            if (!BackColor.IsEmpty && !Default)
                g.FillRectangle(BackColor.Brush(), rect);
        }

        internal virtual Style Combine(Style other) => this;

        internal virtual Style Clone() => Cloned != null ? Cloned : this;

        internal virtual Style FullClone() => (Style)MemberwiseClone();

        internal Style DefaultStyle { get; set; }

        internal Style Cloned { get; set; }

        public Color ForeColor { get; set; }

        public Color BackColor { get; set; }

        public FontStyle FontStyle { get; set; }

        internal bool Default { get; set; }
    }
}
