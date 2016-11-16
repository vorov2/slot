using CodeBox.Autocomplete;
using CodeBox.Folding;
using CodeBox.Affinity;
using CodeBox.ObjectModel;
using CodeBox.Styling;
using System;
using CodeBox.Indentation;
using CodeBox.CallTips;

namespace CodeBox
{
    public interface IEditorContext
    {
        AffinityManager AffinityManager { get; }

        CallTipManager CallTips { get; }//?

        StyleManager Styles { get; }//?

        AutocompleteManager Autocomplete { get; }

        FoldingManager Folding { get; }

        ScrollingManager Scroll { get; }

        CommandManager Commands { get; }

        DocumentBuffer Buffer { get; }

        EditorInfo Info { get; }

        EditorSettings Settings { get; }

        bool WordWrap { get; }

        int WordWrapColumn { get; }

        bool UseTabs { get; }

        int IndentSize { get; }

        bool Overtype { get; set; }

        Pos Caret { get; }
    }
}
