using Slot.Core;
using Slot.Core.Modes;
using Slot.Core.ViewModel;
using Slot.Editor;

namespace Slot.Main.StatusBar
{
    public sealed class ModeTile : StatusBarTile
    {
        private readonly EditorControl editor;
        private ModeMetadata lastMode;

        public ModeTile(EditorControl editor) : base(TileAlignment.Right)
        {
            this.editor = editor;
        }

        public override string Text
        {
            get
            {
                if (editor.Buffer == null)
                    return "";

                if ((lastMode == null && editor.Buffer.GrammarKey != null)
                    || (editor.Buffer.GrammarKey != lastMode.Key && editor.Buffer.GrammarKey != null))
                {
                    lastMode = App.Component<IModeManager>().GetMode(editor.Buffer.GrammarKey);
                    return lastMode.Name;
                }
                else
                    return lastMode.Name;
            }
            set { base.Text = value; }
        }

        protected internal override void PerformClick()
        {
            App.Ext.Run(editor, Cmd.ChangeMode);
            base.PerformClick();
        }
    }

}
