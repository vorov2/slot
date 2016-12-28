using System;
using Slot.Core.ComponentModel;

namespace Slot.Core.CommandModel
{
    public interface ICommandBar : IComponent
    {
        void Show();

        void Show(string commandAlias, params object[] args);

        void ToggleMessage();

        void Hide();

        bool InputVisible { get; }
    }
}
