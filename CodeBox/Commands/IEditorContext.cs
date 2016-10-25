using CodeBox.ObjectModel;
using System;

namespace CodeBox.Commands
{
    public interface IEditorContext
    {
        CommandManager CommandManager { get; }

        DocumentBuffer Buffer { get; }

        EditorInfo Info { get; }

        EditorSettings Settings { get; }
    }
}
