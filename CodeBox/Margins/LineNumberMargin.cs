using System;
using System.Drawing;
using Slot.Drawing;
using Slot.Core.Themes;

namespace Slot.Editor.Margins
{
    public sealed class LineNumberMargin : GutterMargin
    {
        public LineNumberMargin(EditorControl editor) : base(editor)
        {

        }

        protected override bool OnDraw(Graphics g, Rectangle bounds)
        {
            OnSizeChanged();

            if (!Enabled)
                return false;

            var sc = Editor.Scroll.ScrollPosition;
            var lns = Editor.Theme.GetStyle(StandardStyle.LineNumbers);
            var alns = Editor.Theme.GetStyle(StandardStyle.CurrentLineNumber);

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
                var x = bounds.X + info.CharWidth;
                var y = line.Y + sc.Y + info.TextTop;

                if (!Editor.Folding.IsLineVisible(i))
                    continue;

                if (line.Y >= sc.Y && y >= bounds.Y)
                {
                    var str = (i + 1).ToString().PadLeft(len);
                    var col = lns.ForeColor.Brush();

                    if (i == caret.Line && MarkCurrentLine)
                    {
                        var selBrush = alns.BackColor.Brush();

                        if (selBrush != backBrush)
                            g.FillRectangle(selBrush, new Rectangle(bounds.X, y, bounds.Width, info.LineHeight));

                        col = alns.ForeColor.Brush();
                    }

                    g.DrawString(str, font, col, x, y, TextFormats.Compact);
                }
            }

            return true;
        }

        public bool Enabled => Editor.ShowLineNumbers;

        public override int CalculateSize() =>
            Enabled ? (Editor.Document.Lines.Count.ToString().Length + 2) * Editor.Info.CharWidth : 0;

        public bool MarkCurrentLine { get; set; }
    }
}
