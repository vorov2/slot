using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;

namespace CodeBox.Margins
{
    public sealed class LineNumberMargin : Margin
    {
        private int lastScrollY = -1;

        public override void Click(int x, int y, int lineIndex, EditorContext context)
        {
            context.Document.Selections.Clear();
            var sel = context.Document.Selections.Main;
            sel.Start = new Pos(lineIndex, 0);
            sel.End = new Pos(lineIndex, context.Document.Lines[lineIndex].Length);
        }

        public override bool Draw(Graphics g, Rectangle bounds, EditorContext ctx)
        {
            var sc = ctx.Scroll;

            //if (lastScrollY == sc.Y)
            //    return false;
            lastScrollY = sc.Y;
            var lines = ctx.Document.Lines;
            var info = ctx.Info;
            var len = lines.Count.ToString().Length;
            _width = (len + 3) * info.CharWidth;
            var line = lines[0];
            var lineIndex = 0;
            var caret = ctx.Document.Selections.Main.Caret;

            g.FillRectangle(ctx.Renderer.GetBrush(Editor.BackgroundColor), 
                new Rectangle(bounds.X - sc.X, bounds.Y - sc.Y, _width, info.ClientHeight));
            var sb = new StringBuilder();
            do
            {
                if (line.Y >= info.ClientHeight - sc.Y)
                    return true;

                if (line.Y >= info.TopMargin - sc.Y)
                {
                    var str = (lineIndex + 1).ToString().PadLeft(len);
                    var x = bounds.X + info.CharWidth - sc.X;

                    if (lineIndex == caret.Line && MarkCurrentLine)
                        x -= info.CharWidth;

                    g.DrawString(str, info.Font, ctx.Renderer.GetBrush(Editor.GrayColor), x, line.Y);

                    sb.Append(str);
                }

                if (lines.Count == lineIndex + 1)
                    return true;

                line = lines[++lineIndex];
            } while (true);
        }

        private int _width;
        public override int Width
        {
            get { return _width; }
        }

        public bool MarkCurrentLine { get; set; }
    }
}
