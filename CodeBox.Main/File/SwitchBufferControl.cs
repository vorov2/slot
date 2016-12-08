using CodeBox.Core.ViewModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeBox.Main.File
{
    public sealed class SwitchBufferControl : Control
    {
        internal static Font font = new Font(SystemFonts.CaptionFont.Name, 10);

        private static readonly StringFormat format = new StringFormat(StringFormat.GenericTypographic)
        {
            LineAlignment = StringAlignment.Near,
            Alignment = StringAlignment.Near,
            Trimming = StringTrimming.EllipsisPath,
            FormatFlags = StringFormatFlags.NoWrap
        };

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
            var g = e.Graphics;
            var x = 6;
            var y = 6;

            if (SelectedIndex >= 10)
                y -= font.Height * (SelectedIndex - 9);

            for (var i = 0; i < Buffers.Count; i++)
            {
                var b = Buffers[i];
                var rect = new Rectangle(x, y, ClientSize.Width - 12, font.Height);

                if (i == SelectedIndex)
                    g.FillRectangle(SystemBrushes.Highlight, new Rectangle(x - 6, y, ClientSize.Width, font.Height));

                g.DrawString(b.File.Name + " (" + b.File.Directory + ")",
                    font, SystemBrushes.ControlText, rect, format);
                y += font.Height;
            }
        }

        internal int CalculateHeight()
        {
            var max = Buffers.Count > 10 ? 10 : Buffers.Count;
            return max * font.Height + 12;
        }

        internal List<IMaterialBuffer> Buffers { get; set; }

        internal int SelectedIndex { get; set; }

        public event EventHandler CloseRequested;
        private void OnCloseRequested() => CloseRequested?.Invoke(this, EventArgs.Empty);
    }
}
