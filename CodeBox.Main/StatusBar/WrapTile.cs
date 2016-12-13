using CodeBox.Core;

namespace CodeBox.Main.StatusBar
{
    public sealed class WrapTile : StatusBarTile
    {
        private readonly Editor editor;

        public WrapTile(Editor editor) : base(TileAlignment.Right)
        {
            this.editor = editor;
        }

        public override string Text
        {
            get { return "Wrap"; }
            set { base.Text = value; }
        }

        protected internal override void PerformClick()
        {
            App.Ext.Run(editor, Cmd.ToggleWordWrap);
            base.PerformClick();
        }
    }
}
