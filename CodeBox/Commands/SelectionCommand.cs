using System;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionExponent;

namespace CodeBox.Commands
{
    public abstract class SelectionCommand : Command
    {
        public override bool Execute(CommandArgument arg, Selection sel)
        {
            var pos = Select(sel);
            sel.End = pos;
            return true;
        }

        protected abstract Pos Select(Selection sel);
    }
}
