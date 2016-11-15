using System;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionResults;

namespace CodeBox.Commands
{
    public sealed class ScrollLineDownCommand : Command
    {
        public override ActionResults Execute(Selection sel)
        {
            Context.Scroll.ScrollY(-1);
            return Clean | AutocompleteKeep;
        }
    }
}
