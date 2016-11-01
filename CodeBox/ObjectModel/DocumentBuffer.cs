using CodeBox.Commands;
using CodeBox.Styling;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.ObjectModel
{
    public sealed class DocumentBuffer
    {
        internal DocumentBuffer(Document doc)
        {
            Document = doc;
            Selections = new SelectionList();
            UndoStack = new Stack<CommandInfo>();
            RedoStack = new Stack<CommandInfo>();
            Tips = new List<CallTip>();
        }

        public string GetText()
        {
            return string.Join(Eol.AsString(), Document.Lines.Select(ln => ln.Text));
        }

        internal List<CallTip> Tips { get; }

        internal Stack<CommandInfo> UndoStack { get; }

        internal Stack<CommandInfo> RedoStack { get; }

        public Document Document { get; internal set; }

        public SelectionList Selections { get; }

        public int Edits { get; set; }

        public bool Overtype { get; set; }

        public bool WordWrap { get; set; }

        private Eol _eol;
        public Eol Eol
        {
            get
            {
                return _eol == Eol.Auto ? Document.OriginalEol : _eol;
            }
            set { _eol = value; }
        }
    }
}
