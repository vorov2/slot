using System;

namespace CodeBox.Core.ComponentModel
{
    public interface ICommandComponent : IComponent
    {
        void Run(IExecutionContext ctx);
    }
}
