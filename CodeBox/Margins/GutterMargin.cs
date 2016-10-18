using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;

namespace CodeBox.Margins
{
    public class GutterMargin : Margin
    {
        public override MarginEffects MouseDown(Point loc, EditorContext context)
        {
            context.Document.Selections.Clear();
            var sel = context.Document.Selections.Main;
            var lineIndex = context.Editor.FindLineByLocation(loc.Y);

            if (lineIndex >= 0)
            {
                sel.Start = new Pos(lineIndex, 0);
                sel.End = new Pos(lineIndex, context.Document.Lines[lineIndex].Length);
            }

            return MarginEffects.Redraw;
        }

        public override bool Draw(Graphics g, Rectangle bounds, EditorContext ctx)
        {
            g.FillRectangle(ctx.Renderer.GetBrush(Editor.BackgroundColor),
                new Rectangle(bounds.X - ctx.Scroll.X, bounds.Y - ctx.Scroll.Y, CalculateSize(ctx), bounds.Height));
            return true;
        }

        public override int CalculateSize(EditorContext ctx)
        {
            return ctx.Info.CharWidth;
        }
    }
}
