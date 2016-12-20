using System;
using System.Runtime.Serialization;

namespace Slot.Editor.ObjectModel
{
    [Serializable]
    internal class JsonException : Exception
    {
        public JsonException()
        {
        }

        public JsonException(string message) : base(message)
        {
        }

        public JsonException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected JsonException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}