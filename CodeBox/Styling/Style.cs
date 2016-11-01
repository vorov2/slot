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

        internal void DrawAll(Graphics g, Rectangle rect, Pos pos)
        {
            DrawBackground(g, rect, pos);
            DrawText(g, rect, pos);
            DrawAdornment(g, rect, pos);
        }

        public virtual void DrawText(Graphics g, Rectangle rect, Pos pos)
        {
            var ch = Editor.Lines[pos.Line].CharAt(pos.Col);

            if (ch == '\0' && Editor.Settings.ShowEol) ch = '\u00B6';
            else if (ch == '\t' && Editor.Settings.ShowWhitespace) ch = '\u2192';
            else if (ch == ' ' && Editor.Settings.ShowWhitespace) ch = '·';

            g.DrawString(ch.ToString(),
                Font,
                Editor.CachedBrush.Create(ForeColor),
                rect.Location, Format);
        }

        public virtual void DrawAdornment(Graphics g, Rectangle rect, Pos pos)
        {

        }

        public virtual void DrawBackground(Graphics g, Rectangle rect, Pos pos)
        {
            if (!BackColor.IsEmpty && BackColor != Editor.Styles.Default.BackColor)
                g.FillRectangle(Editor.CachedBrush.Create(BackColor), rect);
        }

        internal virtual Style Combine(Style other)
        {
            return this;
        }

        internal virtual Style Clone()
        {
            return Cloned != null ? Cloned : this;
        }

        internal virtual Style FullClone()
        {
            return (Style)MemberwiseClone();
        }

        internal Editor Editor { get; set; }

        internal Style Cloned { get; set; }

        public Color ForeColor { get; set; }

        public Color BackColor { get; set; }

        public FontStyle FontStyle { get; set; }

        internal Brush ForeBrush
        {
            get { return Editor.CachedBrush.Create(ForeColor); }
        }

        internal Brush BackBrush
        {
            get { return Editor.CachedBrush.Create(BackColor); }
        }

        internal Font Font
        {
            get { return Editor.CachedFont.Create(FontStyle); }
        }
    }
}
