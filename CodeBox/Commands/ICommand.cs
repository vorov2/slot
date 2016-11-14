using System;
using CodeBox.ObjectModel;

namespace CodeBox.Commands
{
    public interface ICommand
    {
        ActionResults Execute(CommandArgument arg, Selection sel);

        ActionResults Undo(out Pos pos);

        ActionResults Redo(out Pos pos);

        ICommand Clone();

        IEditorContext Context { get; set; }
    }
}
