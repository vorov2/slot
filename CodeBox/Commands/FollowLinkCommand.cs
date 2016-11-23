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
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.followlink")]
    public sealed class FollowLinkCommand : EditorCommand
    {
        internal override ActionResults Execute(Selection sel, params object[] args)
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

        internal override bool SingleRun => true;
    }
}
