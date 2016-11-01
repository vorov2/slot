using CodeBox.ObjectModel;
using CodeBox.Styling;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CodeBox
{
    public sealed class CallTipManager
    {
        private readonly Editor editor;
        private Pos shownTip = Pos.Empty;
        private AppliedStyle lastHint;

        internal CallTipManager(Editor editor)
        {
            this.editor = editor;
        }

        public void ShowCallTip(Pos pos, string text)
        {
            using (var g = editor.CreateGraphics())
            {
                var size = g.MeasureString(text, editor.Font);
                ShowCallTip(g, pos, size.ToSize(), (gr,pt) => gr.DrawString(text, editor.Font,
                    editor.Styles.Default.ForeBrush, pt));
            }
        }

        public void ShowCallTip(Pos pos, Size size, Action<Graphics,Point> draw)
        {
            using (var g = editor.CreateGraphics())
            {
                ShowCallTip(g, pos, size, draw);
            }
        }

        public void ShowCallTip(Graphics g, Pos pos, Size size, Action<Graphics,Point> draw)
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

                var pt = new Point(x, y);
                g.FillRectangle(editor.CachedBrush.Create(ColorTranslator.FromHtml("#2D2D30")),
                    new Rectangle(pt, size));
                draw(g, pt);
                shownTip = pos;
            }
        }

        public void HideCallTip()
        {
            if (shownTip != Pos.Empty)
            {
                editor.Refresh();
                shownTip = Pos.Empty;
            }
        }

        internal void MouseDwell(Pos p)
        {
            var ln = editor.Lines[p.Line];
            var tip = false;
            var a = ln.FindHyperlink(p.Col);

            if (a != AppliedStyle.Empty)
            {
                if (a.Start != lastHint.Start || a.End != lastHint.End)
                {
                    ShowCallTip(p, "Ctrl + Click to follow link");
                    lastHint = a;
                }

                tip = true;
            }

            if (!tip)
            {
                HideCallTip();
                lastHint = default(AppliedStyle);
            }
        }
    }
}
