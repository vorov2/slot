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

        class ItemInfo
        {
            public string Text;
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
            scrollBar.Size = editor.Info.CharWidth;
            var width = 0;
            var y = 0;

            foreach (var ii in items)
            {
                var w = ii.Text.Length * editor.Info.CharWidth + editor.Info.CharWidth * 2;
                ii.Y = y;

                if (w > width)
                    width = w;

                y += editor.Info.LineHeight;
            }

            width += editor.Info.CharWidth * 5;
            var maxw = editor.Info.TextWidth / 3;
            Width = width > maxw ? maxw : width;
            var fullHeight = editor.Info.CharWidth * 2 + items.Count * editor.Info.LineHeight;
            Height = editor.Info.CharWidth * 2 + (items.Count > 10 ? 10 : items.Count) * editor.Info.LineHeight;
            ScrollMax = fullHeight - Height;
            selectedLine = 0;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;

            var borderPen = editor.CachedPen.Create(editor.Settings.PopupBorderColor);
            g.FillRectangle(editor.CachedBrush.Create(editor.Settings.PopupBackColor), e.ClipRectangle);
            g.DrawRectangle(borderPen,
                e.ClipRectangle.Location.X, e.ClipRectangle.Location.Y,
                e.ClipRectangle.Width - borderPen.Width, e.ClipRectangle.Height - borderPen.Width);

            if (items != null)
            {
                for (var i = 0; i < items.Count; i++)
                {
                    var s = items[i];
                    var y = s.Y + editor.Info.CharWidth + ScrollPosition;

                    if (y >= editor.Info.CharWidth && y + editor.Info.LineHeight < Height)
                    {
                        if (hoverLine == i || selectedLine == i)
                        {
                            g.FillRectangle(editor.CachedBrush.Create(
                                selectedLine == i ? editor.Settings.PopupSelectedColor 
                                    : editor.Settings.PopupHoverColor),
                                new Rectangle(0, y, Width - editor.Info.CharWidth, editor.Info.LineHeight));
                        }

                        g.DrawString(s.Text, Font, editor.CachedBrush.Create(editor.Settings.PopupForeColor),
                            new Rectangle(editor.Info.CharWidth, y, Width - editor.Info.CharWidth*2, editor.Info.LineHeight),
                            Style.Format);
                    }
                }
            }

            scrollBar.Size = editor.Info.CharWidth;
            var border = (int)Math.Round(borderPen.Width, MidpointRounding.AwayFromZero);
            scrollBar.Draw(g, new Rectangle(
                Width - editor.Info.CharWidth - border,
                border,
                editor.Info.CharWidth,
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
            var linesPerScreen = (Height - editor.Info.CharWidth * 2) / editor.Info.LineHeight;
            var fvl = Math.Abs(ScrollPosition / editor.Info.LineHeight);
            var lvl = fvl + linesPerScreen - 1;

            if (line < fvl)
                ScrollY(fvl - line);
            else if (line > lvl)
                ScrollY(lvl - line);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.X >= Width - editor.Info.CharWidth)
            {
                scrollBar.MouseUp(e.Location);
                Invalidate();
            }
            else
                base.OnMouseUp(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.X >= Width - editor.Info.CharWidth)
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
            if (e.X >= Width - editor.Info.CharWidth || scrollBar.IsMouseDown)
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
            return (y - editor.Info.CharWidth - ScrollPosition) / editor.Info.LineHeight;
        }

        public void ScrollY(int times)
        {
            SetScrollPositionY(ScrollPosition + times * editor.Info.LineHeight);
        }

        public void SetScrollPositionY(int value)
        {
            if (value > 0)
                value = 0;

            if (value < -ScrollMax)
                value = -ScrollMax;

            var lines = (int)Math.Round((double)value / editor.Info.LineHeight);
            value = lines * editor.Info.LineHeight;
            ScrollPosition = value;
            Invalidate();
        }

        public void SetItems(IEnumerable<string> items)
        {
            this.items = items.Select(i => new ItemInfo { Text = i }).ToList();
            InvalidateWindow();
        }

        internal int ScrollPosition { get; private set; }

        internal int ScrollMax { get; private set; }

        internal string SelectedItem
        {
            get { return items[selectedLine].Text; }
        }
    }
}
