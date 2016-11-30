using System;
using CodeBox.Core.ComponentModel;
using CodeBox.Core.ViewModel;

namespace CodeBox.Core.CommandModel
{
    public interface ICommandBar : IComponent
    {
        void Show(IExecutionContext ctx);

        void Show(IExecutionContext ctx, string commandAlias, params object[] args);

        void Hide(IExecutionContext ctx);
    }
}
