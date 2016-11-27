using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;
using CodeBox.Styling;
using CodeBox.Folding;
using CodeBox.Drawing;

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
            var sc = Editor.Scroll.ScrollPosition;
            var lns = (MarginStyle)Editor.Styles.Theme.GetStyle(StandardStyle.LineNumbers);

            var lines = Editor.Document.Lines;
            var info = Editor.Info;
            var len = lines.Count.ToString().Length;
            var caret = Editor.Buffer.Selections.Main.Caret;
            var backBrush = lns.BackColor.Brush();
            var font = Editor.Settings.Font.Get(lns.FontStyle);
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
                    var x = bounds.X + info.CharWidth*2;
                    var col = lns.ForeColor.Brush();

                    if (i == caret.Line && MarkCurrentLine)
                    {
                        var selBrush = lns.ActiveBackColor.Brush();

                        if (selBrush != backBrush)
                            g.FillRectangle(selBrush, new Rectangle(bounds.X + info.CharWidth, y, bounds.Width, info.LineHeight));

                        col = lns.ActiveForeColor.Brush();
                    }

                    g.DrawString(str, font, col, x, y);
                }
            }

            return true;
        }

        public override int CalculateSize() =>
            (Editor.Document.Lines.Count.ToString().Length + 4) * Editor.Info.CharWidth;

        public bool MarkCurrentLine { get; set; }
    }
}
