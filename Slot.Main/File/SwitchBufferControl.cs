using Slot.Core;
using Slot.Core.Settings;
using Slot.Core.Themes;
using Slot.Core.ViewModel;
using Slot.Drawing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Slot.Main.File
{
    public sealed class SwitchBufferControl : Control
    {
        public SwitchBufferControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.UserPaint | ControlStyles.Selectable, true);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (ModifierKeys == Keys.None)
                OnCloseRequested();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            SelectedIndex++;

            if (SelectedIndex >= Buffers.Count)
                SelectedIndex = 0;

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var view = App.Catalog<IViewManager>().Default().GetActiveView();
            var theme = App.Catalog<ITheme>().Default();
            var bag = view.Settings.Get<EnvironmentSettings>();
            var style  = theme.GetStyle(StandardStyle.Popup);
            var style1 = theme.GetStyle(StandardStyle.PopupSelected);
            var style2 = theme.GetStyle(StandardStyle.PopupBorder);

            var g = e.Graphics;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            var xPad = Dpi.GetHeight(3);
            var yPad = Dpi.GetHeight(3);
            var x = xPad;
            var y = yPad;
            var height = (int)Math.Round(bag.Font.Height() * 1.1, MidpointRounding.AwayFromZero);

            if (SelectedIndex >= 10)
                y -= height * (SelectedIndex - 9);

            g.FillRectangle(style.BackColor.Brush(), e.ClipRectangle);
            var pen = style2.ForeColor.Pen();
            g.DrawRectangle(pen, new Rectangle(e.ClipRectangle.Location,
                new Size(e.ClipRectangle.Width - pen.Size(), e.ClipRectangle.Height - pen.Size())));
            var ws = App.Catalog<IViewManager>().Default().GetActiveView().Workspace;

            for (var i = 0; i < Buffers.Count; i++)
            {
                if (y > 0 && y < ClientSize.Height - yPad)
                {
                    var b = Buffers[i];
                    var ic = style.ForeColor;

                    if (i == SelectedIndex)
                    {
                        g.FillRectangle(style1.BackColor.Brush(), new Rectangle(x - xPad + Dpi.GetWidth(1), y,
                            ClientSize.Width - 2 * Dpi.GetWidth(1), height));
                        ic = style1.ForeColor.IsEmpty ? style.ForeColor : style1.ForeColor;
                    }

                    var font = bag.Font;
                    var size = g.MeasureString(b.File.Name, font.Get(FontStyle.Bold));
                    g.DrawString(b.File.Name, font.Get(FontStyle.Bold), ic.Brush(), x, y);

                    x += (int)size.Width;
                    var dirName = GetDirectoryName(ws, b.File.Directory);
                    g.DrawString(dirName, font, ic.Brush(),
                        new RectangleF(x, y, ClientSize.Width - xPad * 2 - x, bag.Font.Height()), TextFormats.Path);
                    x = xPad;
                }

                y += height;
            }
        }

        private string GetDirectoryName(DirectoryInfo ws, DirectoryInfo dir)
        {
            var seq = new List<string>();

            do
            {
                if (string.Equals(dir.FullName, ws.FullName, StringComparison.OrdinalIgnoreCase))
                    break;

                seq.Add(dir.Name.Trim('/', '\\'));
                dir = dir.Parent;
            } while (dir != null);

            seq.Reverse();
            return string.Join(Path.DirectorySeparatorChar.ToString(), seq);
        }

        internal int CalculateHeight()
        {
            var max = Buffers.Count > 10 ? 10 : Buffers.Count;
            var bag = App.Catalog<IViewManager>().Default().GetActiveView().Settings.Get<EnvironmentSettings>();
            var height = (int)Math.Round(bag.Font.Height() * 1.1, MidpointRounding.AwayFromZero);
            return max * height + Dpi.GetHeight(3) * 2;
        }

        internal List<IBuffer> Buffers { get; set; }

        internal int SelectedIndex { get; set; }

        public event EventHandler CloseRequested;
        private void OnCloseRequested() => CloseRequested?.Invoke(this, EventArgs.Empty);
    }
}
