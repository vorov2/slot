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
        public abstract void Draw(Graphics g, Point loc, Pos pos);
    }

    public sealed class TextStyle : Style
    {
        private readonly Editor editor;
        private static readonly StringFormat format = new StringFormat(StringFormat.GenericTypographic)
        {
            LineAlignment = StringAlignment.Near,
            Alignment = StringAlignment.Near,
            Trimming = StringTrimming.None
        };

        internal TextStyle(Editor editor, Color fore, Color back, FontStyle fontStyle)
        {
            this.editor = editor;
            ForeColor = fore;
            BackColor = back;
            FontStyle = fontStyle;
        }

        public override void Draw(Graphics g, Point loc, Pos pos)
        {
            var ch = editor.Lines[pos.Line][pos.Col].Char;

            if (BackColor != editor.BackColor)
                g.FillRectangle(editor.CachedBrush.Create(BackColor),
                    new Rectangle(loc, new Size(ch == '\t' ?
                        editor.Settings.TabSize * editor.Info.CharWidth : editor.Info.CharWidth,
                        editor.Info.LineHeight)));

            g.DrawString(ch.ToString(),
                editor.CachedFont.Create(FontStyle),
                editor.CachedBrush.Create(BackColor),
                loc, format);
        }

        public Color ForeColor { get; }

        public Color BackColor { get; }

        public FontStyle FontStyle { get; }
    }
}
