using System;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionExponent;

namespace CodeBox.Commands
{
    public abstract class SelectionCommand : Command
    {
        public override ActionChange Execute(CommandArgument arg, Selection sel)
        {
            var pos = Select(sel);
            sel.End = pos;
            return ActionChange.None;
        }

        protected abstract Pos Select(Selection sel);
    }
}
