using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionExponent;
using CodeBox.Styling;
using System.Diagnostics;

namespace CodeBox.Commands
{
    [CommandBehavior(LeaveEditor)]
    public sealed class FollowLinkCommand : Command
    {
        public override bool Execute(CommandArgument arg, Selection sel)
        {
            var ln = Document.Lines[arg.Pos.Line];
            var a = ln.FindHyperlink(arg.Pos.Col);

            if (a != AppliedStyle.Empty)
            {
                var link = ln.GetRange(a.Start, a.End - a.Start + 1).MakeString();
                Process.Start(link);
                return true;
            }

            return false;
        }
    }
}
