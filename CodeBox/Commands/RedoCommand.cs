using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;

namespace CodeBox.Commands
{
    [CommandBehavior(ActionExponent.None)]
    public sealed class RedoCommand : Command
    {
        public override void Execute(CommandArgument arg, Selection sel)
        {
            Context.CommandManager.Redo();
        }
    }
}
