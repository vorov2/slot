using System;

namespace CodeBox.Main.StatusBar
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
                return editor.Buffer.Overtype ? "OVR" : "INS";
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
