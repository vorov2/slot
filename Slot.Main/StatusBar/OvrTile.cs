using System;
using Slot.Editor;

namespace Slot.Main.StatusBar
{
    public sealed class OvrTile : StatusBarTile
    {
        private readonly EditorControl editor;

        public OvrTile(EditorControl editor) : base(TileAlignment.Right)
        {
            this.editor = editor;
        }

        public override string Text
        {
            get
            {
                return editor.Buffer != null && editor.Buffer.Overtype ? "OVR" : "INS";
            }
            set { base.Text = value; }
        }

        protected internal override void PerformClick()
        {
            editor.Buffer.Overtype = !editor.Buffer.Overtype;
            editor.Focus();
        }
    }
}
