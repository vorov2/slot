using System;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionResults;

namespace CodeBox.Commands
{
    public sealed class SelectAllCommand : Command
    {
        public override ActionResults Execute(Selection sel)
        {
            var idx = Document.Lines.Count - 1;
            var ln = Document.Lines[idx];
            sel.Start = default(Pos);
            sel.End = new Pos(idx, ln.Length);
            return Clean | SingleRun;
        }
    }
}
