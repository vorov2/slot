using System;
using System.Windows.Forms;
using Slot.Core.ComponentModel;
using Slot.Core.ViewModel;

namespace Slot.Core.CommandModel
{
    public interface ICommandBar : IComponent
    {
        void Show();

        void Show(string commandAlias, params object[] args);

        void ToggleMessage();

        void Hide();
    }
}
