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
            var caret = ctx.Document.Selections.Main.Caret;

            g.FillRectangle(ctx.Renderer.Create(Editor.BackgroundColor), bounds);
            var sb = new StringBuilder();
            var y = bounds.Y;
            
            for (var i = ctx.Editor.FirstVisibleLine; i < ctx.Editor.LastVisibleLine + 1; i++)
            {
                var line = lines[i];

                if (line.Y >= info.EditorBottom - sc.Y)
                    return true;

                if (line.Y >= sc.Y && y<= bounds.Height)
                {
                    var str = (i + 1).ToString().PadLeft(len);
                    var x = bounds.X + info.CharWidth - sc.X;

                    if (i == caret.Line && MarkCurrentLine)
                        x -= info.CharWidth;

                    g.DrawString(str, info.Font, ctx.Renderer.Create(Editor.GrayColor), x + sc.X, y);
                    sb.Append(str);
                    y += info.LineHeight;
                }
            }

            return true;
        }

        public override int CalculateSize(EditorContext ctx)
        {
            return (ctx.Document.Lines.Count.ToString().Length + 3) * ctx.Info.CharWidth;
        }

        public bool MarkCurrentLine { get; set; }
    }
}
