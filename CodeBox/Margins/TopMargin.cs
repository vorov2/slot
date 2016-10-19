using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;

namespace CodeBox.Margins
{
    public class TopMargin : Margin
    {
        public TopMargin(Editor editor) : base(editor)
        {

        }

        public override bool Draw(Graphics g, Rectangle bounds)
        {
            g.FillRectangle(Editor.CachedBrush.Create(Editor.BackgroundColor), bounds);
            return true;
        }

        public override int CalculateSize()
        {
            return Editor.Info.LineHeight;
        }
    }
}
