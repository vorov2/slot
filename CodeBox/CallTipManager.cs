using CodeBox.ObjectModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox
{
    public sealed class CallTipManager
    {
        private readonly Editor editor;
        private Pos shownTip = Pos.Empty;

        internal CallTipManager(Editor editor)
        {
            this.editor = editor;
        }

        public void ShowCallTip(Pos pos, Size size)
        {
            if (shownTip != Pos.Empty && shownTip != pos)
                HideCallTip();

            if (pos != shownTip)
            {
                var ln = editor.Lines[pos.Line];
                var x = 0;
                var p = 0;

                foreach (var c in ln)
                {
                    x += c.Char == '\t' ? editor.Settings.TabSize * editor.Info.CharWidth : editor.Info.CharWidth;

                    if (p++ == pos.Col)
                        break;
                }

                x = x + editor.Info.TextLeft + editor.Scroll.X;
                var y = ln.Y + editor.Info.TextTop + editor.Scroll.Y + editor.Info.LineHeight;

                if (y + size.Height > editor.Info.TextHeight)
                    y -= editor.Info.LineHeight + size.Height;

                if (x + size.Width > editor.Info.TextWidth)
                    x -= size.Width;

                using (var g = editor.CreateGraphics())
                    g.FillRectangle(editor.CachedBrush.Create(ColorTranslator.FromHtml("#2D2D30")),
                        new Rectangle(new Point(x, y), size));
                shownTip = pos;
            }
        }

        public void HideCallTip()
        {
            editor.Redraw();
            shownTip = Pos.Empty;
        }
    }
}
