using CodeBox.Commands;
using CodeBox.Styling;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CodeBox.CallTips;

namespace CodeBox.ObjectModel
{
    public sealed class DocumentBuffer
    {
        internal DocumentBuffer(Document doc)
        {
            Document = doc;
            Selections = new SelectionList();
            UndoStack = new LimitedStack<CommandInfo>();
            RedoStack = new LimitedStack<CommandInfo>();
            Tips = new List<CallTip>();
        }

        internal string GetText() =>
            string.Join(Eol.AsString(), Document.Lines.Select(ln => ln.Text));

        public string[] GetLines() => Document.Lines.Select(ln => ln.Text).ToArray();

        internal List<CallTip> Tips { get; }

        internal LimitedStack<CommandInfo> UndoStack { get; }

        internal LimitedStack<CommandInfo> RedoStack { get; }

        public Document Document { get; internal set; }

        public SelectionList Selections { get; }

        internal int Edits { get; set; }

        public bool Overtype { get; set; }

        public bool? WordWrap { get; set; }

        public int? WordWrapColumn { get; set; }

        public bool? UseTabs { get; set; }

        public int? IndentSize { get; set; }

        public bool? ShowEol { get; set; }

        public bool? ShowWhitespace { get; set; }

        public bool? ShowLineLength { get; set; }

        public bool? CurrentLineIndicator { get; set; }

        public bool ReadOnly { get; set; }

        public bool IsDirty
        {
            get { return Edits > 0; }
        }

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
