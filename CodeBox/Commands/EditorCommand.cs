using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionResults;
using CodeBox.Core.CommandModel;

namespace CodeBox.Commands
{
    public abstract class EditorCommand
    {
        internal abstract ActionResults Execute(Selection sel, params object[] args);

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

        protected T GetArg<T>(int num, object[] args, T def = default(T))
        {
            if (args == null || args.Length <= num)
                return def;

            var obj = args[num];
            object res;
            return Converter.Convert(obj, typeof(T), out res) ? (T)res : def;
        }

        internal Editor View { get; set; }

        internal bool GroupUndo { get; set; }

        internal virtual bool SingleRun => false;

        internal virtual bool ModifyContent => false;

        internal virtual bool SupportLimitedMode => false;

        internal virtual EditorCommand Clone() => this;

        protected DocumentBuffer Buffer => View.Buffer;

        protected Document Document => View.Buffer.Document;

        protected EditorSettings Settings => View.Settings;
    }
}
