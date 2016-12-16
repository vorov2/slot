using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slot.Drawing
{
    public static class GraphicsExtensions
    {
        public static void DrawRoundedRectangle(this Graphics g, Color color, Rectangle bounds, int cornerRadius = 5)
        {
            var drawPen = color.Pen();
            var strokeOffset = (int)Math.Ceiling(drawPen.Width);
            bounds = Rectangle.Inflate(bounds, -strokeOffset, -strokeOffset);
            drawPen.EndCap = drawPen.StartCap = LineCap.Round;

            var path = new GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, cornerRadius, cornerRadius, 180, 90);
            path.AddArc(bounds.X + bounds.Width - cornerRadius, bounds.Y, cornerRadius, cornerRadius, 270, 90);
            path.AddArc(bounds.X + bounds.Width - cornerRadius, bounds.Y + bounds.Height - cornerRadius, cornerRadius, cornerRadius, 0, 90);
            path.AddArc(bounds.X, bounds.Y + bounds.Height - cornerRadius, cornerRadius, cornerRadius, 90, 90);
            path.CloseAllFigures();

            g.FillPath(color.Brush(), path);
            g.DrawPath(drawPen, path);
        }
    }
}
