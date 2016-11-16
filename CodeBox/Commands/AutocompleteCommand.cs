using System;
using CodeBox.ObjectModel;
using CodeBox.Autocomplete;
using static CodeBox.Commands.ActionResults;

namespace CodeBox.Commands
{
    public sealed class AutocompleteCommand : Command
    {
        public override ActionResults Execute(Selection sel)
        {
            return Pure | SingleRun | AutocompleteShow;
        }
    }
}
