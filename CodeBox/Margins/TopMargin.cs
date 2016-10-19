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
        public override bool Draw(Graphics g, Rectangle bounds, EditorContext ctx)
        {
            g.FillRectangle(ctx.Renderer.Create(Editor.BackgroundColor), bounds);
            return true;
        }

        public override int CalculateSize(EditorContext ctx)
        {
            return ctx.Info.LineHeight;
        }
    }
}
