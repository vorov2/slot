using System;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionResults;

namespace CodeBox.Commands
{
    public sealed class DeleteWordBackCommand : DeleteBackCommand, IModifyContent
    {
        public override ActionResults Execute(Selection sel)
        {
            var ln = Document.Lines[sel.Caret.Line];

            if (sel.Caret.Col == 0)
                return base.Execute(sel);

            var seps = Context.Settings.NonWordSymbols;
            var st = SelectWordCommand.GetStrategy(seps, ln.CharAt(sel.Caret.Col - 1));
            var col = SelectWordCommand.FindBoundLeft(seps, ln, sel.Caret.Col - 1, st);
            sel.End = new Pos(sel.Caret.Line, col != 0 ? col + 1 : col);
            return base.Execute(sel);
        }

        public override ICommand Clone()
        {
            return new DeleteWordBackCommand();
        }
    }
}
