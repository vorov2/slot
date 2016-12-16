using Slot.Core;
using Slot.Editor;

namespace Slot.Main.StatusBar
{
    public sealed class HelpTile : StatusBarTile
    {
        private readonly EditorControl editor;

        public HelpTile(EditorControl editor) : base(TileAlignment.Right)
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
            App.Ext.Run(editor, (Identifier)"app.commandPalette");
            base.PerformClick();
        }
    }
}
