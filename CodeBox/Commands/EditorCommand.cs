using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionResults;
using System.Windows.Forms;
using CodeBox.ComponentModel;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    public abstract class EditorCommand : ICommand
    {
        public bool Run(IExecutionContext ctx, object arg = null)
        {
            return true;
        }

        internal abstract ActionResults Execute(Selection sel, object arg = null);

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

        internal IEditorView View { get; set; }

        internal virtual bool SingleRun => false;

        internal virtual bool ModifyContent => false;

        internal virtual bool SupportLimitedMode => false;

        internal virtual EditorCommand Clone() => this;

        protected DocumentBuffer Buffer => View.Buffer;

        protected Document Document => View.Buffer.Document;

        protected EditorSettings Settings => View.Settings;
    }
}
