using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Slot.Editor
{
    public sealed class LineEditor : EditorControl
    {
        private readonly EditorControl editor;

        public LineEditor(EditorControl editor) : base(editor.Settings)
        {
            LimitedMode = true;
            this.editor = editor;
            AdjustHeight();
        }

        public void AdjustHeight()
        {
            var h = editor.Info.LineHeight;

            if (Height != h)
                Height = h;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            AdjustHeight();
            base.OnPaint(e);
        }
    }
}
