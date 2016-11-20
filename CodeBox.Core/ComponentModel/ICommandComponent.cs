using System;

namespace CodeBox.Core.ComponentModel
{
    public interface ICommandComponent : IComponent
    {
        bool Run(IExecutionContext ctx);
    }
}
