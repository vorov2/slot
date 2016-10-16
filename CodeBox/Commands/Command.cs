using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;

namespace CodeBox.Commands
{
    public abstract class Command : ICommand
    {
        public abstract void Execute(EditorContext context, Selection sel);

        public virtual Pos Undo(EditorContext context)
        {
            return Pos.Empty;
        }

        public virtual Pos Redo(EditorContext context)
        {
            return Pos.Empty;
        }

        public virtual ICommand Clone()
        {
            return (ICommand)MemberwiseClone();
        }
    }
}
