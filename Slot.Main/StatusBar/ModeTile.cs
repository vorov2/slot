using Slot.Core;
using Slot.Editor;

namespace Slot.Main.StatusBar
{
    public sealed class ModeTile : StatusBarTile
    {
        private readonly EditorControl editor;
        private string lastGrammar;

        public ModeTile(EditorControl editor) : base(TileAlignment.Right)
        {
            this.editor = editor;
        }

        public override string Text
        {
            get
            {
                return (lastGrammar == null && editor.Buffer.GrammarKey != null)
                        || (editor.Buffer.GrammarKey != lastGrammar && editor.Buffer.GrammarKey != null)
                    ? (lastGrammar = App.Ext.Grammars().GetGrammar(editor.Buffer.GrammarKey).Name)
                    : lastGrammar;
            }
            set { base.Text = value; }
        }

        protected internal override void PerformClick()
        {
            App.Ext.Run(editor, (Identifier)"app.changeMode");
            base.PerformClick();
        }
    }

}
