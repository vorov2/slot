using System;
using Slot.Core.ComponentModel;
using Slot.Core.ViewModel;

namespace Slot.Core.Output
{
    public interface ILogComponent : IBuffer, IComponent
    {
        void Write(string text, EntryType type);

        event EventHandler<LogEventArgs> EntryWritten;
    }
}
