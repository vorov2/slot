using System;
using Slot.Core.Messages;

namespace Slot.Main.Messages
{
    public sealed class MessageButtonEventArgs : EventArgs
    {
        public MessageButtonEventArgs(MessageButtons button)
        {
            Button = button;
        }

        public MessageButtons Button { get; }
    }
}
