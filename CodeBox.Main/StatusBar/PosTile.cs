using Slot.Core;
using System;
using Slot.Editor;

namespace Slot.Main.StatusBar
{
    public sealed class PosTile : StatusBarTile
    {
        private readonly EditorControl editor;

        public PosTile(EditorControl editor) : base(TileAlignment.Left)
        {
            this.editor = editor;
        }

        public override string Text
        {
            get
            {
                var caret = editor.Buffer.Selections.Main.Caret;
                return $"Ln {caret.Line + 1}, Ch {caret.Col + 1}";
            }
            set { base.Text = value; }
        }

        protected internal override void PerformClick()
        {
            App.Ext.Run(editor, Cmd.GotoLine);
            base.PerformClick();
        }
    }
}
