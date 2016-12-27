using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Slot.Core;
using Slot.Core.Themes;
using Slot.Core.ViewModel;
using Slot.Drawing;

namespace Slot.Main.Messages
{
    public partial class MessageWindow : Form
    {
        private const int WM_NCHITTEST = 0x84;
        private const int HT_CLIENT = 0x1;
        private const int HT_CAPTION = 0x2;

        public MessageWindow()
        {
            ShowInTaskbar = ControlBox = MinimizeBox = MaximizeBox = false;
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterParent;
            Padding = new Padding(Dpi.GetWidth(3), Dpi.GetHeight(10), Dpi.GetWidth(3), Dpi.GetWidth(3));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var theme = App.Catalog<ITheme>().Default();
            var g = e.Graphics;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.FillRectangle(theme.GetStyle(StandardStyle.PopupBorder).ForeColor.Brush(), ClientRectangle);
            g.FillRectangle(theme.GetStyle(StandardStyle.Popup).BackColor.Brush(),
                new Rectangle(ClientRectangle.X + Padding.Left, ClientRectangle.Y + Padding.Top,
                    ClientRectangle.Width - Padding.Right - Padding.Left,
                    ClientRectangle.Height - Padding.Top - Padding.Bottom));

            var env = App.Catalog<IViewManager>().Default().GetActiveView().Settings.Get<EnvironmentSettings>();
            var padx = Dpi.GetWidth(30);
            var pady = Dpi.GetWidth(20);

            using (var bigfont = new Font(env.FontName, env.FontSize + 3))
            {
                var x = Padding.Left + padx;
                var y = Padding.Top + pady;
                var width = Width - Padding.Left - Padding.Right - padx * 2;
                var size = g.MeasureString(Caption, bigfont, width, TextFormats.Wrap);
                g.DrawString(Caption, bigfont, theme.GetStyle(StandardStyle.Keyword).ForeColor.Brush(), 
                    new Rectangle(x, y, width, Height), TextFormats.Wrap);
            }

            base.OnPaint(e);
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WM_NCHITTEST)
                m.Result = (IntPtr)(HT_CAPTION);
        }

        public string Caption { get; set; }

        public string Detail { get; set; }
    }
}
