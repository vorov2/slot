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
        void Execute(EditorContext context, Selection sel);

        Pos Undo(EditorContext context);

        Pos Redo(EditorContext context);

        ICommand Clone();
    }
}
