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
        public LineNumberMargin(Editor editor) : base(editor)
        {

        }

        public override bool Draw(Graphics g, Rectangle bounds)
        {
            OnSizeChanged();
            var sc = new Point(Editor.Scroll.X, Editor.Scroll.Y);

            var lines = Editor.Document.Lines;
            var info = Editor.Info;
            var len = lines.Count.ToString().Length;
            var caret = Editor.Document.Selections.Main.Caret;

            g.FillRectangle(Editor.CachedBrush.Create(Editor.BackgroundColor), bounds);
            var sb = new StringBuilder();
            var y = bounds.Y;
            
            for (var i = Editor.Scroll.FirstVisibleLine; i < Editor.Scroll.LastVisibleLine + 1; i++)
            {
                var line = lines[i];

                if (line.Y >= info.EditorBottom - sc.Y)
                    return true;

                if (line.Y >= sc.Y)// && y<= bounds.Height)
                {
                    var str = (i + 1).ToString().PadLeft(len);
                    var x = bounds.X + info.CharWidth - sc.X;

                    if (i == caret.Line && MarkCurrentLine)
                        x -= info.CharWidth;

                    g.DrawString(str, info.Font, Editor.CachedBrush.Create(Editor.GrayColor), x + sc.X, y);
                    sb.Append(str);
                    y += info.LineHeight;
                }
            }

            return true;
        }

        public override int CalculateSize()
        {
            return (Editor.Document.Lines.Count.ToString().Length + 3) * Editor.Info.CharWidth;
        }

        public bool MarkCurrentLine { get; set; }
    }
}
