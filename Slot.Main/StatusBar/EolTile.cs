using Slot.Core;
using System;
using Slot.Editor;

namespace Slot.Main.StatusBar
{
    public sealed class EolTile : StatusBarTile
    {
        private readonly EditorControl editor;

        public EolTile(EditorControl editor) : base(TileAlignment.Right)
        {
            this.editor = editor;
        }

        public override string Text
        {
            get
            {
                return editor.Buffer?.Eol.ToString().ToUpper();
            }
            set { base.Text = value; }
        }

        protected internal override void PerformClick()
        {
            App.Ext.Run(editor, Editor.Cmd.SetBufferEol);
            base.PerformClick();
        }
    }

}
