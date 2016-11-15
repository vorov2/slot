using System;
using System.Diagnostics;
using CodeBox.ObjectModel;
using CodeBox.Styling;
using static CodeBox.Commands.ActionResults;

namespace CodeBox.Commands
{
    public sealed class FollowLinkCommand : Command
    {
        public override ActionResults Execute(Selection sel)
        {
            var pos = Context.Caret;
            var ln = Document.Lines[pos.Line];
            var a = ln.FindHyperlink(pos.Col);

            if (a != AppliedStyle.Empty)
            {
                var link = ln.GetRange(a.Start, a.End - a.Start + 1).MakeString();
                Process.Start(link);
                return Clean | LeaveEditor | SingleRun | IdleCaret;
            }

            return Pure;
        }
    }
}
