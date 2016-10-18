using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;

namespace CodeBox.Margins
{
    public sealed class LineNumberMargin : GutterMargin
    {
        public override bool Draw(Graphics g, Rectangle bounds, EditorContext ctx)
        {
            var sc = ctx.Scroll;

            var lines = ctx.Document.Lines;
            var info = ctx.Info;
            var len = lines.Count.ToString().Length;
            var line = lines[0];
            var lineIndex = 0;
            var caret = ctx.Document.Selections.Main.Caret;

            g.FillRectangle(ctx.Renderer.GetBrush(Editor.BackgroundColor), 
                new Rectangle(bounds.X - sc.X, bounds.Y - sc.Y, CalculateSize(ctx), info.ClientHeight));
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

        public override int CalculateSize(EditorContext ctx)
        {
            return (ctx.Document.Lines.Count.ToString().Length + 3) * ctx.Info.CharWidth;
        }

        public bool MarkCurrentLine { get; set; }
    }
}
