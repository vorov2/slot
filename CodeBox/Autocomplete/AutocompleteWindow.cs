using CodeBox.Core.CommandModel;
using CodeBox.Core.ComponentModel;
using CodeBox.Drawing;
using CodeBox.Styling;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeBox.Autocomplete
{
    public sealed class AutocompleteWindow : Control
    {
        private readonly Editor editor;
        private List<ItemInfo> items;
        private readonly AutocompleteScrollBar scrollBar;
        private int hoverLine;
        private int selectedLine;
        private StringFormat format = new StringFormat(StringFormat.GenericTypographic)
        {
            LineAlignment = StringAlignment.Near,
            Alignment = StringAlignment.Near,
            Trimming = StringTrimming.None
        };

        class ItemInfo
        {
            public ValueItem Item;
            public int Y;
        }

        public AutocompleteWindow(Editor editor)
        {
            this.editor = editor;
            SetStyle(ControlStyles.Selectable, false);
            SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.AllPaintingInWmPaint, true);
            Cursor = Cursors.Default;
            scrollBar = new AutocompleteScrollBar(editor, this);
        }

        private void InvalidateWindow()
        {
            scrollBar.Size = CharWidth;
            var width = 0;
            var y = 0;

            foreach (var ii in items)
            {
                var w = ii.Item.ToString().Length * CharWidth + CharWidth * 2;
                ii.Y = y;

                if (w > width)
                    width = w;

                y += LineHeight;
            }

            width += CharWidth * 5;
            var maxw = editor.Info.TextWidth / 3;
            Width = PreferredWidth != null ? PreferredWidth.Value : width > maxw ? maxw : width;
            var fullHeight = CharWidth * 2 + items.Count * LineHeight;
            Height = CharWidth * 2 + (items.Count > MaxItems ? MaxItems : items.Count) * LineHeight;
            ScrollMax = fullHeight - Height;
            ScrollPosition = 0;
            selectedLine = 0;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;

            var ps = (PopupStyle)editor.Styles.Theme.GetStyle(StandardStyle.Popup);
            var borderPen = ps.BorderColor.Pen();
            g.FillRectangle(ps.BackColor.Brush(), e.ClipRectangle);
            g.DrawRectangle(borderPen,
                e.ClipRectangle.Location.X, e.ClipRectangle.Location.Y,
                e.ClipRectangle.Width - borderPen.Width, e.ClipRectangle.Height - borderPen.Width);
            format.Trimming = Trimming;

            if (items != null)
            {
                for (var i = 0; i < items.Count; i++)
                {
                    var s = items[i];
                    var y = s.Y + CharWidth + ScrollPosition;

                    if (y >= CharWidth && y + LineHeight < Height)
                    {
                        if (hoverLine == i || selectedLine == i)
                        {
                            g.FillRectangle((selectedLine == i ? ps.SelectedColor : ps.HoverColor).Brush(),
                                new Rectangle(0, y, Width - CharWidth, LineHeight));
                        }

                        g.DrawString(s.Item.ToString(),
                            SmallFont ? editor.Settings.SmallFont : editor.Settings.Font,
                            ps.ForeColor.Brush(),
                            new Rectangle(CharWidth, y, Width - CharWidth*2, LineHeight),
                            format);

                        if (s.Item.Meta != null)
                        {
                            var mstr = s.Item.Meta.ToString();
                            var x = Width - mstr.Length * CharWidth - CharWidth * 2;
                            foreach (var mc in mstr)
                            {
                                g.DrawString(mc.ToString(),
                                    SmallFont ? editor.Settings.SmallFont : editor.Settings.Font,
                                    ps.ForeColor.Brush(),
                                    new Point(x, y),
                                    format);
                                x += CharWidth;
                            }
                        }
                    }
                }
            }

            scrollBar.Size = CharWidth;
            var border = (int)Math.Round(borderPen.Width, MidpointRounding.AwayFromZero);
            scrollBar.Draw(g, new Rectangle(
                Width - CharWidth - border,
                border,
                CharWidth,
                Height - border*2));
        }

        internal void SelectDown()
        {
            selectedLine++;

            if (selectedLine == items.Count)
            {
                selectedLine = 0;
                ScrollPosition = 0;
            }

            CheckLineVisible(selectedLine);
            Invalidate();
        }

        internal void SelectUp()
        {
            selectedLine--;

            if (selectedLine < 0)
            {
                selectedLine = items.Count - 1;
                ScrollPosition = -ScrollMax;
            }

            CheckLineVisible(selectedLine);
            Invalidate();
        }

        private void CheckLineVisible(int line)
        {
            var linesPerScreen = (Height - CharWidth * 2) / LineHeight;
            var fvl = Math.Abs(ScrollPosition / LineHeight);
            var lvl = fvl + linesPerScreen - 1;

            if (line < fvl)
                ScrollY(fvl - line);
            else if (line > lvl)
                ScrollY(lvl - line);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.X >= Width - CharWidth)
            {
                scrollBar.MouseUp(e.Location);
                Invalidate();
            }
            else
                base.OnMouseUp(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.X >= Width - CharWidth)
            {
                scrollBar.MouseDown(e.Location);
                Invalidate();
            }
            else
            {
                var ln = LocationToLine(e.Location.Y);
                selectedLine = ln;
                base.OnMouseDown(e);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.X >= Width - CharWidth || scrollBar.IsMouseDown)
            {
                if (e.Button != MouseButtons.Left)
                    scrollBar.MouseUp(e.Location);

                scrollBar.MouseMove(e.Location);
            }
            else
                hoverLine = LocationToLine(e.Location.Y);

            Invalidate();
            base.OnMouseMove(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            ScrollY((e.Delta / 120) * 2);
        }

        private int LocationToLine(int y)
        {
            return (y - CharWidth - ScrollPosition) / LineHeight;
        }

        public void ScrollY(int times)
        {
            SetScrollPositionY(ScrollPosition + times * LineHeight);
        }

        public void SetScrollPositionY(int value)
        {
            if (value > 0)
                value = 0;

            if (value < -ScrollMax)
                value = -ScrollMax;

            var lines = (int)Math.Round((double)value / LineHeight);
            value = lines * LineHeight;
            ScrollPosition = value;
            Invalidate();
        }

        public void SetItems(IEnumerable<ValueItem> items)
        {
            this.items = items.Select(i => new ItemInfo { Item = i }).ToList();
            InvalidateWindow();
        }

        public void Reset()
        {
            items.Clear();
            ScrollPosition = 0;
        }

        internal int ScrollPosition { get; private set; }

        internal int ScrollMax { get; private set; }

        internal int? PreferredWidth { get; set; }

        internal int MaxItems { get; set; } = 10;

        internal bool SmallFont { get; set; }

        internal StringTrimming Trimming { get; set; } = StringTrimming.Character;

        internal int CharWidth => SmallFont ? editor.Info.SmallCharWidth : editor.Info.CharWidth;

        internal int CharHeight => SmallFont ? editor.Info.SmallCharHeight : editor.Info.CharHeight;

        internal int LineHeight => CharHeight + (int)Math.Round(CharHeight * editor.Settings.LinePadding);

        internal ValueItem SelectedItem
        {
            get { return items[selectedLine].Item; }
        }
    }
}
