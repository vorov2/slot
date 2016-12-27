using Slot.Editor.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slot.Editor.ObjectModel;
using Slot.Core.ComponentModel;
using System.ComponentModel.Composition;

namespace Slot.Editor.Search
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.searchselect")]
    public sealed class SearchSelectCommand : EditorCommand
    {
        internal override ActionResults Execute(Selection sel, params object[] args)
        {
            var seq = Ed.Search.HasSearchResults ? Ed.Search.EnumerateSearchResults()
                : Ed.MatchWords.HasSearchResults ? Ed.MatchWords.EnumerateSearchResults()
                : null;

            if (seq != null)
            {
                Ed.Buffer.Selections.Clear();

                foreach (var sr in seq)
                {
                    Ed.Buffer.Selections.Add(new Selection(
                        new Pos(sr.Line, sr.StartCol),
                        new Pos(sr.Line, sr.EndCol + 1)
                        ), Ed.Document);
                }
            }

            return ActionResults.Clean | ActionResults.Scroll;
        }

        internal override bool SingleRun => true;
    }
}
