using System;
using System.Diagnostics;
using CodeBox.ObjectModel;
using CodeBox.Styling;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using static CodeBox.Commands.ActionResults;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(ICommand))]
    [CommandData("editor.followlink", "etl")]
    public sealed class FollowLinkCommand : EditorCommand
    {
        protected override ActionResults Execute(Selection sel)
        {
            var pos = View.Caret;
            var ln = Document.Lines[pos.Line];
            var a = ln.FindHyperlink(pos.Col);

            if (a != AppliedStyle.Empty)
            {
                var link = ln.GetRange(a.Start, a.End - a.Start + 1).MakeString();
                Process.Start(link);
                return Clean | LeaveEditor | IdleCaret;
            }

            return Pure;
        }

        public override bool SingleRun => true;
    }
}
