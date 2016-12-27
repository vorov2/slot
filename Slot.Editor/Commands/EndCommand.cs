using System;
using Slot.Editor.ObjectModel;
using System.ComponentModel.Composition;
using Slot.Core.ComponentModel;

namespace Slot.Editor.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.end")]
    public class EndCommand : CaretCommand
    {
        protected override Pos GetPosition(Selection sel)
        {
            var pos = MoveEnd(Ed, sel.Caret);
            sel.SetToRestore(Document.Lines[pos.Line].GetStripeCol(pos.Col));
            return pos;
        }

        internal static Pos MoveEnd(EditorControl ed, Pos pos)
        {
            var ln = ed.Document.Lines[pos.Line];

            if (ed.WordWrap)
            {
                var stripe = ln.GetStripe(pos.Col);
                var cut = ln.GetCut(stripe);
                return new Pos(pos.Line, cut == ln.Length ? cut : cut - 1);
            }
            else
                return new Pos(pos.Line, ln.Length);
        }

        internal override bool SupportLimitedMode => true;
    }
}
