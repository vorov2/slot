using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionResults;

namespace CodeBox.Commands
{
    public abstract class Command : ICommand
    {
        public abstract ActionResults Execute(Selection sel);

        public virtual ActionResults Undo(out Pos pos)
        {
            pos = Pos.Empty;
            return None;
        }

        public virtual ActionResults Redo(out Pos pos)
        {
            pos = Pos.Empty;
            return None;
        }

        public virtual ICommand Clone() => this;

        public IEditorContext Context { get; set; }

        protected DocumentBuffer Buffer => Context.Buffer;

        protected Document Document => Context.Buffer.Document;

        protected EditorSettings Settings => Context.Settings;
    }
}
