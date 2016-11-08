using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(Modify | RestoreCaret | Scroll | Undoable)]
    public sealed class DeleteWordCommand : DeleteCommand
    {
        public override ActionResults Execute(CommandArgument arg, Selection sel)
        {
            var ln = Document.Lines[sel.Caret.Line];

            if (sel.Caret.Col == ln.Length)
                return base.Execute(arg, sel);

            var seps = Context.Settings.NonWordSymbols;
            var st = SelectWordCommand.GetStrategy(seps, ln.CharAt(sel.Caret.Col));
            var col = SelectWordCommand.FindBoundRight(seps, ln, sel.Caret.Col, st);
            var newSel = new Selection(sel.Caret, new Pos(sel.Caret.Line, col));
            return base.Execute(arg, newSel);
        }
    }
}
