using System;

namespace CodeBox.StatusBar
{
    public sealed class OvrTile : StatusBarTile
    {
        private readonly Editor editor;

        public OvrTile(Editor editor) : base(TileAlignment.Right)
        {
            this.editor = editor;
        }

        public override string Text
        {
            get
            {
                return editor.Overtype ? "OVR" : "INS";
            }
            set { base.Text = value; }
        }

        protected internal override void PerformClick()
        {
            editor.Overtype = !editor.Overtype;
            editor.Focus();
        }
    }
}
