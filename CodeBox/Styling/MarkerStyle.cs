using CodeBox.ObjectModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Styling
{

    public sealed class MarkerStyle : Style
    {
        public MarkerStyle()
        {

        }

        public override void Draw(Graphics g, Rectangle rect, Pos pos)
        {
            g.FillRectangle(Editor.CachedBrush.Create(BackColor), rect);
        }

        public Color ForeColor { get; set; }

        public Color BackColor { get; set; }

        internal Brush ForeBrush
        {
            get { return Editor.CachedBrush.Create(ForeColor); }
        }

        internal Brush BackBrush
        {
            get { return Editor.CachedBrush.Create(BackColor); }
        }
    }
}
