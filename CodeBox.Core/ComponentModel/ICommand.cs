using System;

namespace CodeBox.Core.ComponentModel
{
    public interface ICommand : IComponent
    {
        bool Run(IExecutionContext ctx, object arg = null);
    }
}
