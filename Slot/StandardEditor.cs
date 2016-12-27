using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Slot.Editor;
using Slot.Editor.Margins;

namespace Slot
{
    public sealed class StandardEditor : EditorControl
    {
        private readonly List<Control> slaves = new List<Control>();

        public StandardEditor(EditorSettings settings) : base(settings)
        {
            Dock = DockStyle.Fill;
            LeftMargins.Add(new LineNumberMargin(this) { MarkCurrentLine = true });
            LeftMargins.Add(new FoldingMargin(this));
            RightMargins.Add(new VerticalScrollBarMargin(this));
            BottomMargins.Add(new ScrollBarMargin(this, Orientation.Horizontal));
            TopMargins.Add(new TopMargin(this));
        }

        public void AddSlave(Control c) => slaves.Add(c);

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            slaves.ForEach(s => s.Invalidate());
        }
    }
}
