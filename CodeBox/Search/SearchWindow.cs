using CodeBox.Core.Keyboard;
using CodeBox.Drawing;
using CodeBox.ObjectModel;
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
        private static readonly StringFormat format = new StringFormat(StringFormat.GenericTypographic)
        {
            LineAlignment = StringAlignment.Center,
            Alignment = StringAlignment.Near,
            Trimming = StringTrimming.None
        };

        public SearchWindow(Editor editor)
        {
            this.editor = editor;
            Cursor = Cursors.Default;
            SearchBox = new LineEditor(editor);
            SearchBox.Paint += SearchBoxPaint;
            SearchBox.Styles.StyleNeeded += SearchBoxStyling;
            SearchBox.CommandRejected += SearchBoxCommandRejected;
            SearchBox.LostFocus += SearchBoxLostFocus;
            Controls.Add(SearchBox);
        }

        private void SearchBoxCommandRejected(object sender, EventArgs e)
        {
            editor.RunCommand(KeyboardAdapter.Instance.LastKey);
        }

        private void SearchBoxLostFocus(object sender, EventArgs e)
        {
            SearchBox.Redraw();
        }

        private void SearchBoxStyling(object sender, StyleNeededEventArgs e)
        {
            SearchBox.Styles.ClearStyles(0);

            var style = InputInvalid ? StandardStyle.Error
                : UseRegex ? StandardStyle.Regex : StandardStyle.Default;
            SearchBox.Styles.StyleRange(style, 0, 0, SearchBox.Lines[0].Length);
        }

        private void SearchBoxPaint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;

            if (tip != null)
            {
                var width = tip.Length * editor.Info.SmallCharWidth;
                var x = SearchBox.Info.TextWidth - width - editor.Info.SmallCharWidth * 3;
                var style = editor.Styles.Theme.GetStyle(StandardStyle.Popup);
                g.DrawRoundedRectangle(style.BackColor,
                    new Rectangle(x - editor.Info.SmallCharWidth, 0, width + editor.Info.SmallCharWidth * 2, editor.Info.LineHeight));

                foreach (var c in tip)
                {
                    g.DrawString(c.ToString(), editor.Settings.SmallFont, style.ForeColor.Brush(),
                        new Rectangle(x, 0, editor.Info.SmallCharWidth, editor.Info.LineHeight), format);
                    x += editor.Info.SmallCharWidth;
                }
            }
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
                SearchBox.Styles.RestyleDocument();
                OnSettingsChanged();
            }
        }

        private string tip;
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            var loc = e.Location;

            if (OnButton(loc, button1))
            {
                tip = "Ignore case";
                SearchBox.Redraw();
            }
            else if (OnButton(loc, button2))
            {
                tip = "Whole word";
                SearchBox.Redraw();
            }
            else if (OnButton(loc, button3))
            {
                tip = "Regular expressions";
                SearchBox.Redraw();
            }
            else if (tip != null)
            {
                tip = null;
                SearchBox.Redraw();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (tip != null)
            {
                tip = null;
                SearchBox.Redraw();
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
        internal bool InputInvalid { get; set; }

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

        public override Color BackgroundColor => editor.Styles.Theme.GetStyle(StandardStyle.Default).BackColor;

        public override Color BorderColor =>
            ((PopupStyle)editor.Styles.Theme.GetStyle(StandardStyle.Popup)).BorderColor;
    }
}
