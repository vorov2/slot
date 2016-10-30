using CodeBox.ObjectModel;
using System;

namespace CodeBox.Commands
{
    public interface IEditorContext
    {
        CommandManager Commands { get; }

        DocumentBuffer Buffer { get; }

        EditorInfo Info { get; }

        EditorSettings Settings { get; }

        bool AtomicChange { get; set; }
    }
}
