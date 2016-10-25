using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(None)]
    public sealed class AddSelectionCommand : Command
    {
        public override void Execute(CommandArgument arg, Selection sel)
        {
            Buffer.Selections.Add(new Selection(arg.Pos));
        }
    }
}
