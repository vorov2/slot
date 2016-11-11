using System;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionResults;

namespace CodeBox.Commands
{
    public sealed class ScrollLineUpCommand : Command
    {
        public override ActionResults Execute(CommandArgument arg, Selection sel)
        {
            Context.Scroll.ScrollY(1);
            return Clean | AutocompleteKeep;
        }
    }
}
