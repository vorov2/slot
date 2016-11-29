using CodeBox.Drawing;
using CodeBox.Styling;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeBox.Search
{
    public sealed class SearchWindow : Overlay
    {
        private readonly Editor editor;

        public SearchWindow(Editor editor)
        {
            this.editor = editor;
            Cursor = Cursors.Default;
            SearchBox = new LineEditor(editor);
            Controls.Add(SearchBox);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var sb = SearchBox;
            var rect = ClientRectangle;
            sb.Location = new Point(editor.Info.CharWidth / 2, (rect.Height - sb.Height) / 2);
            var optWidth = editor.Info.CharWidth * 11;
            var butWidth = 0;// editor.Info.CharWidth * 7;
            var pad = editor.Info.CharWidth;

            sb.Size = new Size(rect.Width - optWidth - butWidth - pad, sb.Height);

            var g = e.Graphics;

            button1 = new Rectangle(sb.Left + sb.Width, sb.Top, editor.Info.CharWidth * 3, editor.Info.LineHeight);
            DrawButton(g, "Aa", button1, CaseSensitive);

            button2 = new Rectangle(sb.Left + sb.Width + button1.Width, sb.Top, editor.Info.CharWidth * 4, editor.Info.LineHeight);
            DrawButton(g, "[a]", button2, WholeWord);

            button3 = new Rectangle(sb.Left + sb.Width + button1.Width + button2.Width, sb.Top, editor.Info.CharWidth * 4, editor.Info.LineHeight);
            DrawButton(g, ".*", button3, UseRegex);
        }

        private Rectangle button1;
        private Rectangle button2;
        private Rectangle button3;

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            var loc = e.Location;

            if (OnButton(loc, button1))
            {
                CaseSensitive = !CaseSensitive;
                Invalidate();
                OnSettingsChanged();
            }
            else if (OnButton(loc, button2))
            {
                WholeWord = !WholeWord;
                Invalidate();
                OnSettingsChanged();
            }
            else if (OnButton(loc, button3))
            {
                UseRegex = !UseRegex;
                Invalidate();
                OnSettingsChanged();
            }
        }

        private bool OnButton(Point loc, Rectangle button)
        {
            return loc.X >= button.Left && loc.X <= button.Right
                && loc.Y >= button.Top && loc.Y <= button.Bottom;
        }

        public bool CaseSensitive { get; set; }
        public bool UseRegex { get; set; }
        public bool WholeWord { get; set; }

        private void DrawButton(Graphics g, string chars, Rectangle rect, bool set)
        {
            var fnt = editor.Settings.Font;
            var def = editor.Styles.Theme.DefaultStyle;
            var style = editor.Styles.Theme.GetStyle(StandardStyle.SpecialSymbol);

            g.FillRectangle(def.BackColor.Brush(),
                new Rectangle(rect.X, rect.Y, rect.Width, editor.Info.LineHeight));

            var shift = 0;
            var col = set ? def.ForeColor : style.ForeColor;
            g.DrawString(chars[0].ToString(), fnt, col.Brush(), new Point(rect.X + shift, rect.Y));
            shift += editor.Info.SmallCharWidth;
            g.DrawString(chars[1].ToString(), fnt, col.Brush(), new Point(rect.X + shift, rect.Y));

            if (chars.Length > 2)
            {
                shift += editor.Info.SmallCharWidth;
                g.DrawString(chars[2].ToString(), fnt, col.Brush(), new Point(rect.X + shift, rect.Y));
            }
        }

        internal LineEditor SearchBox { get; }

        public event EventHandler SettingsChanged;
        private void OnSettingsChanged() => SettingsChanged?.Invoke(this, EventArgs.Empty);

        public override Color BackgroundColor => editor.Styles.Theme.GetStyle(StandardStyle.CommandBar).BackColor;

        public override Color BorderColor =>
            ((PopupStyle)editor.Styles.Theme.GetStyle(StandardStyle.Popup)).BorderColor;
    }
}
