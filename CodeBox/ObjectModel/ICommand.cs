using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;

namespace CodeBox.ObjectModel
{
    public interface ICommand
    {
        void Execute(CommandArgument arg, Selection sel);

        Pos Undo();

        Pos Redo();

        ICommand Clone();

        IEditorContext Context { get; set; }
    }
}
