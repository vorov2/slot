using CodeBox.Folding;
using CodeBox.ObjectModel;
using CodeBox.Styling;
using System;

namespace CodeBox
{
    public interface IEditorContext
    {
        CallTipManager CallTips { get; }

        StyleManager Styles { get; }

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
