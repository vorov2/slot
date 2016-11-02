using CodeBox.ObjectModel;
using System;

namespace CodeBox.Commands
{
    public interface IEditorContext
    {
        FoldingManager Folding { get; }

        IndentManager Indents { get; }

        ScrollingManager Scroll { get; }

        CommandManager Commands { get; }

        DocumentBuffer Buffer { get; }

        EditorInfo Info { get; }

        EditorSettings Settings { get; }

        bool AtomicChange { get; set; }
    }
}
