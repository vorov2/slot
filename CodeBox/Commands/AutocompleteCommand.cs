using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionExponent;
using CodeBox.Autocomplete;
using System.Windows.Forms;

namespace CodeBox.Commands
{
    [CommandBehavior(SingleRun | Silent | IdleCaret)]
    public sealed class AutocompleteCommand : Command
    {
        private DocumentCompleteSource completeSource = new DocumentCompleteSource();

        public override ActionResults Execute(CommandArgument arg, Selection sel)
        {
            var list = completeSource.GetItems(Context.Buffer);
            Context.Autocomplete.ShowAutocomplete(sel.Caret, list);
            return ActionResults.AutocompleteShow;
        }
    }
}
