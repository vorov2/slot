using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;

namespace CodeBox.Margins
{
    public sealed class GutterMargin : Margin
    {
        public override void Click(int x, int y, int lineIndex, EditorContext context)
        {
            context.Document.Selections.Clear();
            var sel = context.Document.Selections.Main;
            sel.Start = new Pos(lineIndex, 0);
            sel.End = new Pos(lineIndex, context.Document.Lines[lineIndex].Length);
        }

        public override bool Draw(Graphics g, Rectangle bounds, EditorContext ctx)
        {
            _width = ctx.Info.CharWidth;
            g.FillRectangle(ctx.Renderer.GetBrush(Editor.BackgroundColor),
                new Rectangle(bounds.X - ctx.Scroll.X, bounds.Y - ctx.Scroll.Y, _width, ctx.Info.ClientHeight));
            return true;
        }

        private int _width;
        public override int Width
        {
            get { return _width; }
        }

        public bool MarkCurrentLine { get; set; }
    }
}
