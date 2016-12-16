using Slot.Core.ComponentModel;
using System.Collections.Generic;

namespace Slot.Core.CommandModel
{
    public interface ICommandProvider : IComponent
    {
        IEnumerable<CommandMetadata> EnumerateCommands();

        CommandMetadata GetCommandByAlias(string alias);

        CommandMetadata GetCommandByKey(Identifier key);
    }
}