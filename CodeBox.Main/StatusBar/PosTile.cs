using System;

namespace CodeBox.Main.StatusBar
{
    public sealed class PosTile : StatusBarTile
    {
        private readonly Editor editor;

        public PosTile(Editor editor) : base(TileAlignment.Left)
        {
            this.editor = editor;
        }

        public override string Text
        {
            get
            {
                var caret = editor.Buffer.Selections.Main.Caret;
                return $"Ln {caret.Line + 1}, Ch {caret.Col + 1}";
            }
            set { base.Text = value; }
        }

        protected internal override void PerformClick()
        {
            base.PerformClick();
        }
    }
}
