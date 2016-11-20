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
    public class TextStyle : Style
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

        internal virtual TextStyle Combine(TextStyle other)
        {
            var hidden = other.Clone();
            hidden.ForeColor = ForeColor.IsEmpty ? other.ForeColor : ForeColor;
            hidden.BackColor = BackColor.IsEmpty ? other.BackColor : BackColor;
            hidden.FontStyle = FontStyle == FontStyle.Regular ? other.FontStyle : FontStyle;
            //hidden.ForeColor = other.ForeColor.IsEmpty ? ForeColor : other.ForeColor;
            //hidden.BackColor = other.BackColor.IsEmpty ? BackColor : other.BackColor;
            //hidden.FontStyle = other.FontStyle == FontStyle.Regular ? FontStyle : other.FontStyle;
            return hidden;
        }

        internal virtual TextStyle Clone() => Cloned != null ? Cloned : this;

        internal virtual TextStyle FullClone() => (TextStyle)MemberwiseClone();

        internal TextStyle DefaultStyle { get; set; }

        internal TextStyle Cloned { get; set; }

        internal bool Default { get; set; }
    }
}
