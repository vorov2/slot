using System;
using Slot.Editor.ObjectModel;
using System.ComponentModel.Composition;
using Slot.Core.ComponentModel;

namespace Slot.Editor.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.home")]
    public class HomeCommand : CaretCommand
    {
        protected override Pos GetPosition(Selection sel)
        {
            var pos = MoveHome(Ed, sel.Caret);
            sel.SetToRestore(Document.Lines[pos.Line].GetStripeCol(pos.Col));
            return pos;
        }

        internal static Pos MoveHome(EditorControl ed, Pos pos)
        {
            var ln = ed.Document.Lines[pos.Line];
            var stripe = ln.GetStripe(pos.Col);

            if (stripe > 0)
            {
                var cut = ln.GetCut(stripe - 1);
                return new Pos(pos.Line, cut);
            }
            else
            {
                var ch = '\0';

                if (pos.Col > 0 && ((ch = ln.CharAt(pos.Col - 1)) == ' ' || ch == '\t'))
                    return new Pos(pos.Line, 0);

                for (var i = 0; i < ln.Length; i++)
                {
                    var c = ln.CharAt(i);

                    if (c != ' ' && c != '\t')
                        return new Pos(pos.Line, i);
                }

                return new Pos(pos.Line, 0);
            }
        }

        internal override bool SupportLimitedMode => true;
    }
}
