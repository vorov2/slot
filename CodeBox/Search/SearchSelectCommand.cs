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
    [ComponentData("editor.searchselect")]
    public sealed class SearchSelectCommand : EditorCommand
    {
        internal override ActionResults Execute(Selection sel, params object[] args)
        {
            if (View.Search.HasSearchResults)
            {
                View.Buffer.Selections.Clear();

                foreach (var sr in View.Search.EnumerateSearchResults())
                {
                    View.Buffer.Selections.Add(new Selection(
                        new Pos(sr.Line, sr.StartCol),
                        new Pos(sr.Line, sr.EndCol + 1)
                        ));
                }
            }

            return ActionResults.Clean | ActionResults.Scroll;
        }

        internal override bool SingleRun => true;
    }
}
