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

        public virtual Pos Undo() => Pos.Empty;

        public virtual Pos Redo() => Pos.Empty;

        public virtual ICommand Clone() => (ICommand)MemberwiseClone();

        public IEditorContext Context { get; set; }

        protected DocumentBuffer Buffer => Context.Buffer;

        protected Document Document => Context.Buffer.Document;

        protected EditorSettings Settings => Context.Settings;
    }
}
