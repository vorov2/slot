using Slot.Core;

namespace Slot.Main.StatusBar
{
    public sealed class WrapTile : StatusBarTile
    {
        public WrapTile() : base(TileAlignment.Right)
        {

        }

        public override string Text
        {
            get { return "Wrap"; }
            set { base.Text = value; }
        }

        protected internal override void PerformClick()
        {
            App.Ext.Run(Editor.Cmd.ToggleWordWrap);
            base.PerformClick();
        }
    }
}
