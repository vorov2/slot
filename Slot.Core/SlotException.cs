using System;

namespace Slot.Core
{
    public class SlotException : Exception
    {
        public SlotException(string message) : base(message)
        {

        }

        public SlotException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}
