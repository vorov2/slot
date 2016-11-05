using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;
using CodeBox.Styling;
using CodeBox.Folding;

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
            var backBrush = Editor.CachedBrush.Create(Editor.Settings.LineNumbersBackColor);
            var font = Editor.Settings.Font;
            g.FillRectangle(backBrush, bounds);

            for (var i = Editor.Scroll.FirstVisibleLine; i < Editor.Scroll.LastVisibleLine + 1; i++)
            {
                var line = lines[i];
                var y = line.Y + sc.Y + info.TextTop;

                if (line.Folding.Has(FoldingStates.Invisible))
                    continue;

                if (line.Y >= sc.Y && y >= bounds.Y)
                {
                    var str = (i + 1).ToString().PadLeft(len);
                    var x = bounds.X + info.CharWidth - sc.X;
                    var col = Editor.CachedBrush.Create(Editor.Settings.LineNumbersForeColor);

                    if (i == caret.Line && MarkCurrentLine)
                    {
                        var selBrush = Editor.CachedBrush.Create(Editor.Settings.LineNumbersCurrentBackColor);

                        if (selBrush != backBrush)
                            g.FillRectangle(selBrush, new Rectangle(x + sc.X, y, bounds.Width, info.LineHeight));

                        col = Editor.CachedBrush.Create(Editor.Settings.LineNumbersCurrentForeColor);
                    }

                    g.DrawString(str, font, col, x + sc.X, y);
                }
            }

            return true;
        }

        public override int CalculateSize() =>
            (Editor.Document.Lines.Count.ToString().Length + 3) * Editor.Info.CharWidth;

        public bool MarkCurrentLine { get; set; }
    }
}
