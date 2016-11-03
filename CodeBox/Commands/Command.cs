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
        public abstract ActionResults Execute(CommandArgument arg, Selection sel);

        public virtual Pos Undo()
        {
            return Pos.Empty;
        }

        public virtual Pos Redo()
        {
            return Pos.Empty;
        }

        public virtual ICommand Clone()
        {
            return (ICommand)MemberwiseClone();
        }

        public IEditorContext Context { get; set; }

        protected DocumentBuffer Buffer
        {
            get { return Context.Buffer; }
        }

        protected Document Document
        {
            get { return Context.Buffer.Document; }
        }

        protected EditorSettings Settings
        {
            get { return Context.Settings; }
        }
    }
}
