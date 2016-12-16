using System;
using System.Drawing;
using Slot.Drawing;
using System.Windows.Forms;
using Slot.Core.Themes;

namespace Slot.Editor.Margins
{
    public sealed class TopMargin : Margin
    {
        public TopMargin(EditorControl editor) : base(editor)
        {

        }

        protected override bool OnDraw(Graphics g, Rectangle bounds)
        {
            var cs = Editor.Theme.GetStyle(StandardStyle.Default);
            g.FillRectangle(ControlPaint.Dark(cs.BackColor, .05f).Brush(), bounds);
                //ColorTranslator.FromHtml("#161616").Brush(), bounds);//cs.BackColor.Brush(), bounds);
            return true;
        }

        public override int CalculateSize() => (int)Math.Round(Editor.Info.LineHeight * .1);
    }
}
