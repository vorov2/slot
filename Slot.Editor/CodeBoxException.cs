using System;

namespace Slot.Editor
{
    public class CodeBoxException : Exception
    {
        public CodeBoxException(string message) : base(message)
        {

        }

        public CodeBoxException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}
