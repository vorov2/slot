using Slot.Core;
using System;
using Slot.Editor;
using System.Text;

namespace Slot.Main.StatusBar
{
    public sealed class EncodingTile : StatusBarTile
    {
        private readonly EditorControl editor;

        public EncodingTile(EditorControl editor) : base(TileAlignment.Right)
        {
            this.editor = editor;
        }

        public override string Text
        {
            get
            {
                return editor.Buffer.Encoding.WebName.ToUpper();
            }
            set { base.Text = value; }
        }

        protected internal override void PerformClick()
        {
            App.Ext.Run(editor, Cmd.ReopenFile);
            base.PerformClick();
        }
    }

}
