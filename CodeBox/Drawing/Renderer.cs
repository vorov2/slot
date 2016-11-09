using CodeBox.Styling;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Drawing
{
    internal sealed class Renderer
    {
        private readonly Editor editor;
        private static readonly StringFormat format = new StringFormat(StringFormat.GenericTypographic)
        {
            LineAlignment = StringAlignment.Center,
            Alignment = StringAlignment.Center,
            Trimming = StringTrimming.None
        };

        public Renderer(Editor editor)
        {
            this.editor = editor;
        }

        internal int DrawLineLengthIndicator(Graphics g, int len, int x, int y)
        {
            if (x + editor.Scroll.X < editor.Info.TextLeft)
                return 0;

            var str = len.ToString();
            g.DrawString(str, editor.Settings.SmallFont, editor.Styles.SpecialSymbol.ForeBrush,
                new PointF(x, y), Style.Format);
            return (str.Length + 1) * editor.Info.CharWidth;
        }

        internal void DrawCaretIndicator(Graphics g, int line, int col, int x, int y)
        {
            var str = (line + 1) + " " + (col + 1);
            var w = (str.Length + 1) * editor.Info.CharWidth;
            x -= w / 2;

            if (x + editor.Scroll.X < editor.Info.TextLeft)
                return;

            if (y == editor.Info.TextTop)
                y += editor.Info.LineHeight;
            else
                y -= editor.Info.LineHeight;

            var rect = new Rectangle(x, y, w, editor.Settings.SmallFont.Height);
            g.FillRectangle(editor.Styles.Selection.BackBrush, rect);
            g.DrawString(str, editor.Settings.SmallFont, editor.Styles.Default.ForeBrush, rect, format);
        }
    }
}
