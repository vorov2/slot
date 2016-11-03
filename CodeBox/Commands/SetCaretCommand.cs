using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(SingleRun)]
    public sealed class SetCaretCommand : Command
    {
        public override ActionResults Execute(CommandArgument arg, Selection sel)
        {
            Buffer.Selections.Set(new Selection(arg.Pos));
            return ActionResults.Clean;
        }
    }
}
