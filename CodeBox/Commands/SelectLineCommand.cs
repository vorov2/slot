using System;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionResults;

namespace CodeBox.Commands
{
    public sealed class SelectLineCommand : Command
    {
        public override ActionResults Execute(CommandArgument arg, Selection sel)
        {
            if (arg.Pos.Line > -1)
            {
                sel.Start = new Pos(arg.Pos.Line, 0);
                sel.End = new Pos(arg.Pos.Line, Document.Lines[arg.Pos.Line].Length);
            }

            return Clean;
        }
    }
}
