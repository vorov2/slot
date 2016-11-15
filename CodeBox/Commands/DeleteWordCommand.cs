using System;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionResults;

namespace CodeBox.Commands
{
    public sealed class DeleteWordCommand : DeleteCommand, IModifyContent
    {
        public override ActionResults Execute(Selection sel)
        {
            var ln = Document.Lines[sel.Caret.Line];

            if (sel.Caret.Col == ln.Length)
                return base.Execute(sel);

            var seps = Context.Settings.NonWordSymbols;
            var st = SelectWordCommand.GetStrategy(seps, ln.CharAt(sel.Caret.Col));
            var col = SelectWordCommand.FindBoundRight(seps, ln, sel.Caret.Col, st);
            var newSel = new Selection(sel.Caret, new Pos(sel.Caret.Line, col));
            return base.Execute(newSel);
        }

        public override ICommand Clone()
        {
            return new DeleteWordCommand();
        }
    }
}
