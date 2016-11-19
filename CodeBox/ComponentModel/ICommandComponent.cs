using System;

namespace CodeBox.ComponentModel
{
    public interface ICommandComponent : IComponent
    {
        void Run(IExecutionContext context);
    }
}
