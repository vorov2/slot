using Slot.Core;
using Slot.Editor;

namespace Slot.Main.StatusBar
{
    public sealed class WrapTile : StatusBarTile
    {
        private readonly EditorControl editor;

        public WrapTile(EditorControl editor) : base(TileAlignment.Right)
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
