using Slot.Core.ComponentModel;
using Slot.Core.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slot.Core.Output
{
    public interface ILogComponent : IMaterialBuffer, IComponent
    {
        void Write(string text, EntryType type);

        event EventHandler<LogEventArgs> EntryWritten;
    }

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

    public enum EntryType
    {
        Info,

        Warning,

        Error
    }
}
