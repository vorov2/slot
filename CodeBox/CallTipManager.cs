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
        private CallTip lastTip;
        private static readonly StringFormat format = new StringFormat(StringFormat.GenericTypographic)
        {
            LineAlignment = StringAlignment.Center,
            Alignment = StringAlignment.Center,
            Trimming = StringTrimming.None
        };

        internal CallTipManager(Editor editor)
        {
            this.editor = editor;
        }

        public void ShowCallTip(Pos pos, string text)
        {
            using (var g = editor.CreateGraphics())
            {
                var size = g.MeasureString(text + "__", editor.Settings.SmallFont);
                size = new SizeF(size.Width, size.Height * 1.5f);
                ShowCallTip(g, pos, size.ToSize(), (gr,pt) => gr.DrawString(
                    text,
                    editor.Settings.SmallFont,
                    editor.CachedBrush.Create(editor.Settings.PopupForeColor),
                    new Rectangle(pt, size.ToSize()),
                    format));
            }
        }

        public void ShowCallTip(Pos pos, Size size, Action<Graphics,Point> draw)
        {
            using (var g = editor.CreateGraphics())
                ShowCallTip(g, pos, size, draw);
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
                var rect = new Rectangle(pt, size);
                g.FillRectangle(editor.CachedBrush.Create(editor.Settings.PopupBackColor), rect);
                g.DrawRectangle(editor.CachedPen.Create(editor.Settings.PopupBorderColor), rect);
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

        public void ClearCallTips() => editor.Buffer.Tips.Clear();

        public void BindCallTip(string data, Pos start, Pos end) =>
            editor.Buffer.Tips.Add(new CallTip(data, start, end));

        public CallTip FindCallTip(Pos pos)
        {
            foreach (var c in editor.Buffer.Tips)
                if (pos >= c.Start && pos <= c.End)
                    return c;

            return CallTip.Empty;
        }

        internal void MouseDwell(Pos p)
        {
            var ln = editor.Lines[p.Line];
            var tip = false;
            var ct = FindCallTip(p);

            if (ct != CallTip.Empty)
            {
                if (ct.Start != lastTip.Start || ct.End != lastTip.End)
                {
                    ShowCallTip(p, ct.Data);
                    lastTip = ct;
                }

                tip = true;
            }

            if (!tip)
            {
                HideCallTip();
                lastTip = CallTip.Empty;
            }
        }
    }
}
