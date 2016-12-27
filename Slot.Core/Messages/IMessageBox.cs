using System;
using Slot.Core.ComponentModel;

namespace Slot.Core.Messages
{
    public interface IMessageBox : IComponent
    {
        MessageButtons Show(string caption, string text, MessageButtons buttons);
    }
}
