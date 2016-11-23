using CodeBox.Core.ComponentModel;
using System;

namespace CodeBox.Core.CommandModel
{
    public interface ICommandDispatcher : IComponent
    {
        bool Execute(IExecutionContext ctx, Identifier commandKey, params object[] args);
    }
}
