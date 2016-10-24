using CodeBox.ObjectModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Styling
{

    public sealed class TextStyle : Style
    {
        private static readonly StringFormat format = new StringFormat(StringFormat.GenericTypographic)
        {
            LineAlignment = StringAlignment.Near,
            Alignment = StringAlignment.Near,
            Trimming = StringTrimming.None
        };

        public TextStyle()
        {

        }

        public override void Draw(Graphics g, Rectangle rect, Pos pos)
        {
            var ch = Editor.Lines[pos.Line].CharAt(pos.Col);

            if (ch == '\0' && Editor.Settings.ShowEol) ch = '\u00B6';
            else if (ch == '\t' && Editor.Settings.ShowWhitespace) ch = '\u2192';
            else if (ch == ' ' && Editor.Settings.ShowWhitespace) ch = '·';

            if (BackColor != Editor.Styles.Default.BackColor && !BackColor.IsEmpty)
                g.FillRectangle(Editor.CachedBrush.Create(BackColor), rect);

            g.DrawString(ch.ToString(),
                Editor.CachedFont.Create(FontStyle),
                Editor.CachedBrush.Create(ForeColor),
                rect.Location, format);
        }

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
