using System;
using Slot.Core.ComponentModel;
using Slot.Core.ViewModel;

namespace Slot.Core.CommandModel
{
    public interface ICommandBar : IComponent
    {
        void Show(IExecutionContext ctx);

        void Show(IExecutionContext ctx, string commandAlias, params object[] args);

        void Hide(IExecutionContext ctx);
    }
}
