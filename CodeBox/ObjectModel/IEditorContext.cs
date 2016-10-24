using System;

namespace CodeBox.ObjectModel
{
    public interface IEditorContext
    {
        DocumentBuffer Buffer { get; }

        EditorInfo Info { get; }

        EditorSettings Settings { get; }
    }
}
