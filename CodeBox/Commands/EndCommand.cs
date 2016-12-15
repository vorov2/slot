using System;
using CodeBox.ObjectModel;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.end")]
    public class EndCommand : CaretCommand
    {
        protected override Pos GetPosition(Selection sel)
        {
            var pos = MoveEnd(Document, sel.Caret);
            sel.SetToRestore(Document.Lines[pos.Line].GetStripeCol(pos.Col));
            return pos;
        }

        internal static Pos MoveEnd(Document doc, Pos pos)
        {
            var ln = doc.Lines[pos.Line];
            return new Pos(pos.Line, ln.Length);
        }

        internal override bool SupportLimitedMode => true;
    }
}
