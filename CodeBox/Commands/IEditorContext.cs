using CodeBox.ObjectModel;
using System;

namespace CodeBox.Commands
{
    public interface IEditorContext
    {
        AutocompleteManager Autocomplete { get; }

        LocationManager Locations { get; }

        FoldingManager Folding { get; }

        IndentManager Indents { get; }

        ScrollingManager Scroll { get; }

        CommandManager Commands { get; }

        DocumentBuffer Buffer { get; }

        EditorInfo Info { get; }

        EditorSettings Settings { get; }

        bool WordWrap { get; }

        int WordWrapColumn { get; }

        bool Overtype { get; set; }
    }
}
