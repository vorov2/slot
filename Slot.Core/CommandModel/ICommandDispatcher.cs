using Slot.Core.ComponentModel;
using System;

namespace Slot.Core.CommandModel
{
    public interface ICommandDispatcher : IComponent
    {
        bool Execute(IExecutionContext ctx, Identifier commandKey, params object[] args);
    }
}
