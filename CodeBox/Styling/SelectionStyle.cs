using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;

namespace CodeBox.Styling
{
    public sealed class SelectionStyle : Style
    {
        public override void Draw(Graphics g, Rectangle rect, Pos pos)
        {
            if (!Color.IsEmpty)
                g.FillRectangle(Editor.CachedBrush.Create(Color), rect);
        }

        public Color Color { get; set; }
    }
}
