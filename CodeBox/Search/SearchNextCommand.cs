using CodeBox.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;
using CodeBox.Core.ComponentModel;
using System.ComponentModel.Composition;

namespace CodeBox.Search
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.searchnext")]
    public sealed class SearchNextCommand : EditorCommand
    {
        internal override ActionResults Execute(Selection sel, params object[] args)
        {
            if (!View.Search.IsSearchVisible)
                View.Search.ShowSearch();

            var caret = sel.Caret;
            var found = false;

            foreach (var sr in View.Search.EnumerateSearchResults())
                if (sr.Line > caret.Line || (sr.Line == caret.Line && sr.StartCol > caret.Col))
                {
                    View.Buffer.Selections.Set(new Selection(
                        new Pos(sr.Line, sr.StartCol),
                        new Pos(sr.Line, sr.EndCol + 1)
                        ));
                    found = true;
                    break;
                }

            if (!found && caret != new Pos(0, 0))
                return Execute(new Selection(new Pos(0, 0)));

            return ActionResults.Clean | ActionResults.Scroll;
        }

        internal override bool SingleRun => true;
    }
}
