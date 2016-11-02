using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;

namespace CodeBox.Commands
{
    public interface ICommand
    {
        ActionResult Execute(CommandArgument arg, Selection sel);

        Pos Undo();

        Pos Redo();

        ICommand Clone();

        IEditorContext Context { get; set; }
    }
}
