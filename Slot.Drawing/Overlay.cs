using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Slot.Drawing;

namespace Slot.Drawing
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
            var ratioX = Dpi.GetWidth(1);
            var ratioY = Dpi.GetHeight(1);
            g.FillRectangle(BorderColor.Brush(), new Rectangle(0, 0, Width, Height));
            g.FillRectangle(BackgroundColor.Brush(), new Rectangle(ratioX, ratioY,
                Width - ratioX * 2, Height - ratioY * 2));
        }

        public virtual Color BorderColor { get; }

        public virtual Color BackgroundColor { get; }
    }
}
