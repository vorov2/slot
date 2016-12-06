using CodeBox.Core.ComponentModel;
using CodeBox.Core.Output;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.ObjectModel
{
    [Export(typeof(ILog))]
    [ComponentData("log.application")]
    public class LogBuffer : DocumentBuffer, ILog
    {
        public LogBuffer() 
            : base(Document.FromString(""), new FileInfo("memory"), Encoding.UTF8)
        {

        }

        void ILog.Write(string text, EntryType type)
        {
            var ln = Line.FromString(text);
            ln.State = (int)type;
            Document.Lines.Add(ln);
            Selections.Set(new Pos(Document.Lines.Count - 1, ln.Length));
            ScrollToCaret();
            RequestRedraw();
            OnEntryWritten(text, type);
        }

        public event EventHandler<LogEventArgs> EntryWritten;
        private void OnEntryWritten(string text, EntryType type) => EntryWritten?.Invoke(this, new LogEventArgs(text, type));
    }
}
