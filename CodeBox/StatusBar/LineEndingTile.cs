using System;

namespace CodeBox.StatusBar
{
    public sealed class LineEndingTile : StatusBarTile
    {
        private readonly Editor editor;

        public LineEndingTile(Editor editor) : base(TileAlignment.Right)
        {
            this.editor = editor;
        }

        public override string Text
        {
            get
            {
                return editor.Buffer.Eol.ToString().ToUpper();
            }
            set { base.Text = value; }
        }

        protected internal override void PerformClick()
        {
            base.PerformClick();
        }
    }

}
