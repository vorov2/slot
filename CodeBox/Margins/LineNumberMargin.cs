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
    public sealed class LineNumberMargin : GutterMargin
    {
        public LineNumberMargin(Editor editor) : base(editor)
        {

        }

        protected override bool OnDraw(Graphics g, Rectangle bounds)
        {
            OnSizeChanged();
            var sc = new Point(Editor.Scroll.X, Editor.Scroll.Y);

            var lines = Editor.Document.Lines;
            var info = Editor.Info;
            var len = lines.Count.ToString().Length;
            var caret = Editor.Buffer.Selections.Main.Caret;
            var backBrush = Editor.Styles.LineNumber.BackBrush;

            g.FillRectangle(backBrush, bounds);
            var sb = new StringBuilder();
            
            for (var i = Editor.Scroll.FirstVisibleLine; i < Editor.Scroll.LastVisibleLine + 1; i++)
            {
                var line = lines[i];
                var y = line.Y + sc.Y + info.TextTop;

                if (line.Y >= info.TextBotom - sc.Y)
                    return true;

                if (line.Y >= sc.Y && y >= bounds.Y)
                {
                    var str = (i + 1).ToString().PadLeft(len);
                    var x = bounds.X + info.CharWidth - sc.X;
                    var font = Editor.Styles.LineNumber.Font;
                    var col = Editor.Styles.LineNumber.ForeBrush;

                    if (i == caret.Line && MarkCurrentLine)
                    {
                        var selBrush = Editor.Styles.CurrentLineNumber.BackBrush;

                        if (selBrush != backBrush)
                            g.FillRectangle(selBrush, new RectangleF(x + sc.X, y, bounds.Width, info.LineHeight));

                        font = Editor.Styles.CurrentLineNumber.Font;
                        col = Editor.Styles.CurrentLineNumber.ForeBrush;
                    }

                    g.DrawString(str, font, col, x + sc.X, y);
                    sb.Append(str);
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
