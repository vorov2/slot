using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(SingleRun)]
    public sealed class SetCaretCommand : Command
    {
        public override bool Execute(CommandArgument arg, Selection sel)
        {
            Buffer.Selections.Set(new Selection(arg.Pos));
            return true;
        }
    }
}
