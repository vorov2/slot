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
    public sealed class SelectionStyle : Style
    {
        public override void DrawBackground(Graphics g, Rectangle rect, Pos pos) =>
            g.FillRectangle(BackColor.Brush(), rect);

        internal override Style Combine(Style other)
        {
            var hidden = other.Clone();
            hidden.ForeColor = other.ForeColor;
            hidden.BackColor = BackColor;
            hidden.FontStyle = other.FontStyle;
            return hidden;
        }
    }
}
