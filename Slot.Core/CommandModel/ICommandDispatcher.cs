using System;
using Slot.Core.ComponentModel;
using Slot.Core.ViewModel;

namespace Slot.Core.CommandModel
{
    public interface ICommandDispatcher : IComponent
    {
        bool Execute(IEditor editor, Identifier commandKey, params object[] args);
    }
}
