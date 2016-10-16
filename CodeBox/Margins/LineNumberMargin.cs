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

        public override void Click(int x, int y, Line line, EditorContext context)
        {
            context.Document.Selections.Clear();
            var sel = context.Document.Selections.Main;
            sel.Start = new Pos(line.Index, 0);
            sel.End = new Pos(line.Index, line.Length);
        }

        public override bool Draw(Graphics g, Rectangle bounds, EditorContext context)
        {
            var sc = context.Scroll;

            //if (lastScrollY == sc.Y)
            //    return false;
            lastScrollY = sc.Y;
            var lines = context.Document.Lines;
            var info = context.Info;
            var len = lines.Count.ToString().Length;
            _width = (len + 3) * info.CharWidth;
            var line = lines[0];
            var caret = context.Document.Selections.Main.Caret;

            g.FillRectangle(Editor.Background, 
                new Rectangle(bounds.X - sc.X, bounds.Y - sc.Y, _width, info.ClientHeight));
            var sb = new StringBuilder();
            do
            {
                if (line.Y >= info.ClientHeight - sc.Y)
                    return true;

                if (line.Y >= info.TopMargin - sc.Y)
                {
                    var str = (line.Index + 1).ToString().PadLeft(len);
                    var x = bounds.X + info.CharWidth - sc.X;

                    if (line.Index == caret.Line && MarkCurrentLine)
                        x -= info.CharWidth;

                    g.DrawString(str, info.Font, Editor.Gray, x, line.Y);

                    sb.Append(str);
                }

                if (lines.Count == line.Index + 1)
                    return true;

                line = lines[line.Index + 1];
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
