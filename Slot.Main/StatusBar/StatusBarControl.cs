using Slot.Core;
using Slot.Core.Settings;
using Slot.Core.Themes;
using Slot.Drawing;
using Slot.Editor.Margins;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;
using Slot.Editor;
using Slot.Core.ViewModel;

namespace Slot.Main.StatusBar
{
    public sealed class StatusBarControl : Control
    {
        public StatusBarControl(EditorControl editor)
        {
            SetStyle(ControlStyles.Selectable, false);
            SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.AllPaintingInWmPaint | ControlStyles.FixedHeight, true);
            Dock = DockStyle.Bottom;
            Cursor = Cursors.Default;
            Editor = editor;
        }

        private void AdjustHeight()
        {
            var h = (int)Math.Round(
                (double)((IView)FindForm()).Settings.Get<EnvironmentSettings>().Font.Height()
                    + Dpi.GetHeight(4), MidpointRounding.AwayFromZero);

            if (Height != h)
                Height = h;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            AdjustHeight();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            AdjustHeight();
            var g = e.Graphics;
            var bounds = e.ClipRectangle;
            var theme = App.Catalog<ITheme>().Default();
            var style = theme.GetStyle(StandardStyle.StatusBar);
            var astyle = theme.GetStyle(StandardStyle.ActiveStatusBar);
            var font = ((IView)FindForm()).Settings.Get<EnvironmentSettings>().SmallFont;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.FillRectangle(style.BackColor.Brush(), bounds);

            var ys = Dpi.GetHeight(2);
            g.FillRectangle(style.BackColor.Brush(),
                new Rectangle(bounds.X, bounds.Y, bounds.Width, ys));
            var pad = Dpi.GetWidth(6);
            var space = Editor.Info.SmallCharWidth;

            var lefts = Tiles.Where(t => t.Alignment == TileAlignment.Left);
            var x = bounds.X + pad;

            foreach (var tile in lefts)
            {
                var foreColor = style.ForeColor;
                tile.Font = font;
                var width = tile.MeasureWidth(g);

                if (x + width > bounds.Width)
                    break;

                var rect = new Rectangle(x, bounds.Y + ys, width, bounds.Height - ys * 2);

                if (tile.Hover)
                {
                    g.FillRectangle(astyle.BackColor.Brush(), rect);
                    foreColor = astyle.ForeColor;
                }

                tile.Draw(g, foreColor, rect);
                tile.Left = x;
                tile.Right = x + width;
                x += width + space;
            }

            var maxx = x;
            var rights = Tiles.Where(t => t.Alignment == TileAlignment.Right);
            x = bounds.X + bounds.Width - pad;

            foreach (var tile in rights)
            {
                var foreColor = style.ForeColor;
                tile.Font = font;
                var width = tile.MeasureWidth(g);
                x -= width;

                if (x < maxx)
                    break;

                var rect = new Rectangle(x, bounds.Y + ys, width, bounds.Height - ys*2);

                if (tile.Hover)
                {
                    g.FillRectangle(astyle.BackColor.Brush(), rect);
                    foreColor = astyle.ForeColor;
                }

                tile.Left = x;
                tile.Right = x + width;
                tile.Draw(g, foreColor, rect);
                x -= space;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            var tile = Tiles.FirstOrDefault(t => e.X >= t.Left && e.X <= t.Right);
            var hover = Tiles.FirstOrDefault(t => t.Hover);

            if (tile == hover)
                return;

            if (hover != null)
                hover.Hover = false;

            if (tile != null)
                tile.Hover = true;

            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            var hover = Tiles.FirstOrDefault(t => t.Hover);

            if (hover != null)
            {
                hover.Hover = false;
                Invalidate();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            var tile = Tiles.FirstOrDefault(t => e.X >= t.Left && e.X <= t.Right);

            if (tile != null)
                tile.PerformClick();
        }

        public EditorControl Editor { get; }

        public List<StatusBarTile> Tiles { get; } = new List<StatusBarTile>();
    }
}
