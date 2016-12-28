using System;

namespace Slot.Core.Output
{
    public sealed class LogEventArgs : EventArgs
    {
        public LogEventArgs(string data, EntryType type)
        {
            Data = data;
            Type = type;
        }

        public string Data { get; }

        public EntryType Type { get; }
    }
}
