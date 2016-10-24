using System;
using CodeBox.ObjectModel;
using static CodeBox.ObjectModel.ActionExponent;

namespace CodeBox.Commands
{
    internal abstract class SelectionCommand : Command
    {
        public override void Execute(CommandArgument arg, Selection sel)
        {
            var pos = Select(sel.Caret);
            sel.End = pos;
        }

        protected abstract Pos Select(Pos pos);
    }
}
