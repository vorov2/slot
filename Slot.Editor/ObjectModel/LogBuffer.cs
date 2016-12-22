using Slot.Core.Output;
using System;
using System.IO;
using System.Text;

namespace Slot.Editor.ObjectModel
{
    public class LogBuffer : DocumentBuffer, ILogComponent
    {
        public LogBuffer() 
            : base(Document.FromString(""), new FileInfo("memory"), Encoding.UTF8, Guid.NewGuid())
        {

        }

        void ILogComponent.Write(string text, EntryType type)
        {
            var ln = Line.FromString(text);
            ln.State = (int)type;
            Document.Lines.RemoveAt(Document.Lines.Count - 1);
            Document.Lines.Add(ln);
            Document.Lines.Add(Line.Empty());
            Selections.Set(new Pos(Document.Lines.Count - 1, 0));
            InvalidateLines();
            ScrollToCaret();
            RequestRedraw();
            OnEntryWritten(text, type);
        }

        public event EventHandler<LogEventArgs> EntryWritten;
        private void OnEntryWritten(string text, EntryType type) => EntryWritten?.Invoke(this, new LogEventArgs(text, type));
    }
}
