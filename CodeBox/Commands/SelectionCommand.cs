using System;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionExponent;

namespace CodeBox.Commands
{
    public abstract class SelectionCommand : Command
    {
        public override ActionResults Execute(CommandArgument arg, Selection sel)
        {
            var pos = Select(sel);
            sel.End = pos;
            return ActionResults.Clean;
        }

        protected abstract Pos Select(Selection sel);
    }
}
