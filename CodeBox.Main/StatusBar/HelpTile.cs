using CodeBox.Core;

namespace CodeBox.Main.StatusBar
{
    public sealed class HelpTile : StatusBarTile
    {
        private readonly Editor editor;

        public HelpTile(Editor editor) : base(TileAlignment.Right)
        {
            this.editor = editor;
        }

        public override string Text
        {
            get { return "?"; }
            set { base.Text = value; }
        }

        protected internal override void PerformClick()
        {
            App.Ext.Run(editor, (Identifier)"test.commandpalette");
            base.PerformClick();
        }
    }
}
