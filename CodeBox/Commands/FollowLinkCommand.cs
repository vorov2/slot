using System;
using System.Diagnostics;
using Slot.Editor.ObjectModel;
using Slot.Editor.Styling;
using System.ComponentModel.Composition;
using static Slot.Editor.Commands.ActionResults;
using Slot.Core.ComponentModel;

namespace Slot.Editor.Commands
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
