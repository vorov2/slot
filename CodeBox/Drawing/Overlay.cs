using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeBox.Drawing
{
    public class Overlay : Control
    {
        public Overlay()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.Selectable, false);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            var ratioX = (int)Math.Round((g.DpiX / 96f) * 2);
            var ratioY = (int)Math.Round((g.DpiY / 96f) * 2);
            g.FillRectangle(BorderColor.Brush(), new Rectangle(0, 0, Width, Height));
            g.FillRectangle(BackColor.Brush(), new Rectangle(ratioX, 0,
                Width - ratioX * 2, Height - ratioY));
        }

        public Color BorderColor { get; set; }
    }
}
