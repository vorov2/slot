using Slot.Core;

namespace Slot.Main.StatusBar
{
    public sealed class HelpTile : StatusBarTile
    {
        public HelpTile() : base(TileAlignment.Right)
        {

        }

        public override string Text
        {
            get { return "?"; }
            set { base.Text = value; }
        }

        protected internal override void PerformClick()
        {
            App.Ext.Run(Cmd.CommandPalette);
            base.PerformClick();
        }
    }
}
