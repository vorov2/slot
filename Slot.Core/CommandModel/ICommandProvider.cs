using System;
using System.Collections.Generic;
using Slot.Core.ComponentModel;

namespace Slot.Core.CommandModel
{
    public interface ICommandProvider : IComponent
    {
        IEnumerable<CommandMetadata> EnumerateCommands();

        CommandMetadata GetCommandByAlias(string alias);

        CommandMetadata GetCommandByKey(Identifier key);
    }
}