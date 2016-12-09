using CodeBox.Core;
using CodeBox.Core.Settings;
using CodeBox.Core.Themes;
using CodeBox.Core.ViewModel;
using CodeBox.Drawing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace CodeBox.Main.File
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
            var bag = App.Catalog<ISettingsProvider>().First().Get<EditorSettings>();
            var style = App.Catalog<IThemeComponent>().First().GetStyle(StandardStyle.Popup);
            var style1 = App.Catalog<IThemeComponent>().First().GetStyle(StandardStyle.PopupSelected);
            var style2 = App.Catalog<IThemeComponent>().First().GetStyle(StandardStyle.PopupBorder);

            var g = e.Graphics;
            var xPad = bag.SmallFont.Width();
            var yPad = bag.SmallFont.Width();
            var x = xPad;
            var y = yPad;
            var height = (int)Math.Round(bag.Font.Height * 1.1, MidpointRounding.AwayFromZero);

            if (SelectedIndex >= 10)
                y -= height * (SelectedIndex - 9);

            g.FillRectangle(style.BackColor.Brush(), e.ClipRectangle);
            var pen = style2.ForeColor.Pen();
            g.DrawRectangle(pen, new Rectangle(e.ClipRectangle.Location,
                new Size(e.ClipRectangle.Width - (int)pen.Width, e.ClipRectangle.Height - (int)pen.Width)));
            
            for (var i = 0; i < Buffers.Count; i++)
            {
                if (y > 0 && y < ClientSize.Height - yPad)
                {
                    var b = Buffers[i];
                    var ic = style.ForeColor;

                    if (i == SelectedIndex)
                    {
                        g.FillRectangle(style1.BackColor.Brush(), new Rectangle(x - xPad, y,
                            ClientSize.Width, height));
                        ic = style1.ForeColor.IsEmpty ? style.ForeColor : style1.ForeColor;
                    }

                    var font = bag.Font;

                    foreach (var c in b.File.Name)
                    {
                        g.DrawString(c.ToString(), font.Get(FontStyle.Bold), ic.Brush(), new Rectangle(x, y,
                            ClientSize.Width - xPad * 2, font.Height()), TextFormats.Compact);
                        x += bag.Font.Width();
                    }

                    x += bag.Font.Width();
                    g.DrawString(b.File.DirectoryName, font, ic.Brush(),
                        new RectangleF(x, y, ClientSize.Width - xPad * 2 - x, bag.SmallFont.Height()), TextFormats.Path);
                    x = xPad;
                }

                y += height;
            }
        }

        internal int CalculateHeight()
        {
            var max = Buffers.Count > 10 ? 10 : Buffers.Count;
            var bag = App.Catalog<ISettingsProvider>().First().Get<EditorSettings>();
            var height = (int)Math.Round(bag.Font.Height * 1.1, MidpointRounding.AwayFromZero);
            return max * height + bag.SmallFont.Width() * 2;
        }

        internal List<IMaterialBuffer> Buffers { get; set; }

        internal int SelectedIndex { get; set; }

        public event EventHandler CloseRequested;
        private void OnCloseRequested() => CloseRequested?.Invoke(this, EventArgs.Empty);
    }
}
