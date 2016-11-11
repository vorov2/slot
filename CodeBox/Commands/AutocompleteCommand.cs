using System;
using CodeBox.ObjectModel;
using CodeBox.Autocomplete;
using static CodeBox.Commands.ActionResults;

namespace CodeBox.Commands
{
    public sealed class AutocompleteCommand : Command
    {
        private DocumentCompleteSource completeSource = new DocumentCompleteSource();

        public override ActionResults Execute(CommandArgument arg, Selection sel)
        {
            completeSource.Initialize(Context);
            var list = completeSource.GetItems();
            Context.Autocomplete.ShowAutocomplete(sel.Caret, list);
            return Pure | SingleRun | AutocompleteShow;
        }
    }
}
