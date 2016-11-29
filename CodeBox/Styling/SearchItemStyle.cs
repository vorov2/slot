using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;
using CodeBox.Drawing;

namespace CodeBox.Styling
{
    public class SearchItemStyle : TextStyle
    {
        public override void DrawAdornment(Graphics g, Rectangle rect, Pos pos)
        {
            var p = LineColor.ThickPen();
            var pw = (int)Math.Round(p.Width, MidpointRounding.AwayFromZero);
            g.DrawLine(p,
                new Point(rect.X, rect.Y + rect.Height - pw),
                new Point(rect.X + rect.Width, rect.Y + rect.Height - pw));
        }
    }
}
