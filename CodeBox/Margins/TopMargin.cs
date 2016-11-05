using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;
using CodeBox.Styling;

namespace CodeBox.Margins
{
    public class TopMargin : Margin
    {
        public TopMargin(Editor editor) : base(editor)
        {

        }

        protected override bool OnDraw(Graphics g, Rectangle bounds)
        {
            g.FillRectangle(Editor.CachedBrush.Create(ColorTranslator.FromHtml("#2D2D30")), bounds);
            return true;
        }

        public override int CalculateSize() => Editor.Info.LineHeight;
    }
}
