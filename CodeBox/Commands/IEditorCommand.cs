using System;
using CodeBox.ObjectModel;
using CodeBox.ComponentModel;

namespace CodeBox.Commands
{
    public interface ICommandComponent : IComponent
    {
        void Run(IExecutionContext context);
    }

    public interface IEditorCommand : ICommandComponent
    {
        ActionResults Execute(Selection sel);

        ActionResults Undo(out Pos pos);

        ActionResults Redo(out Pos pos);

        IEditorContext Context { get; set; }

        IEditorCommand Clone();
    }
}
