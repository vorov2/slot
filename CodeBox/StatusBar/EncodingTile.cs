using CodeBox.Core;
using CodeBox.Core.ComponentModel;
using System;

namespace CodeBox.StatusBar
{
    public sealed class EncodingTile : StatusBarTile
    {
        private readonly Editor editor;

        public EncodingTile(Editor editor) : base(TileAlignment.Right)
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
            App.Ext.RunCommand(editor, (Identifier)"file.reopenfile");
            base.PerformClick();
        }
    }

}
