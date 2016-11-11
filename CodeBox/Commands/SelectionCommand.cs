using System;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionResults;

namespace CodeBox.Commands
{
    public abstract class SelectionCommand : Command
    {
        public override ActionResults Execute(CommandArgument arg, Selection sel)
        {
            var pos = Select(sel);
            sel.End = pos;
            return Clean | Scroll;
        }

        protected abstract Pos Select(Selection sel);
    }
}
