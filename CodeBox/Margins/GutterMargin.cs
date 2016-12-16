using System;
using System.Drawing;
using Slot.Drawing;
using Slot.Core;
using Slot.Core.Themes;

namespace Slot.Editor.Margins
{
    public class GutterMargin : Margin
    {
        public GutterMargin(EditorControl editor) : base(editor)
        {

        }

        public override MarginEffects MouseDown(Point loc)
        {
            var sel = Editor.Buffer.Selections.Main;
            var lineIndex = Editor.Locations.FindLineByLocation(loc.Y);
            Editor.RunCommand((Identifier)"editor.selectline", lineIndex + 1);
            return MarginEffects.Redraw;
        }

        protected override bool OnDraw(Graphics g, Rectangle bounds)
        {
            g.FillRectangle(Editor.Theme.GetStyle(StandardStyle.Default).BackColor.Brush(), bounds);
            return true;
        }

        public override int CalculateSize() => Editor.Info.CharWidth;
    }
}
