using CodeBox.ObjectModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Styling
{
    public abstract class Style
    {
        internal void DrawAll(Graphics g, Rectangle rect, Pos pos)
        {
            var style = this;

            while (style != null)
            {
                style.Draw(g, rect, pos);
                style = style.NextStyle;
            }
        }

        public abstract void Draw(Graphics g, Rectangle rect, Pos pos);

        internal Editor Editor { get; set; }

        internal Style NextStyle { get; set; }
    }
}
