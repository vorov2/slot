using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(SingleRun)]
    public sealed class SetCaretCommand : Command
    {
        public override void Execute(CommandArgument arg, Selection sel)
        {
            Buffer.Selections.Set(new Selection(arg.Pos));
        }
    }
}
