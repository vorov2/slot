using CodeBox.Commands;
using CodeBox.Styling;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CodeBox.CallTips;
using System.Text;

namespace CodeBox.ObjectModel
{
    public sealed class DocumentBuffer
    {
        private int counter;
        private bool undoGroup;
        private EditorLock editorLock;
        private volatile int lockCount;

        private sealed class EditorLock : IEditorLock
        {
            private readonly DocumentBuffer man;

            internal EditorLock(DocumentBuffer man)
            {
                this.man = man;
            }

            public void Release()
            {
                man.lockCount--;
            }
        }

        public DocumentBuffer(Document doc, string fileName, Encoding encoding)
        {
            Document = doc;
            Selections = new SelectionList(doc);
            UndoStack = new LimitedStack<CommandInfo>();
            RedoStack = new LimitedStack<CommandInfo>();
            Tips = new List<CallTip>();
            Encoding = encoding;
            FileName = fileName;
            editorLock = new EditorLock(this);
        }

        public string GetText()
        {
            var @lock = ObtainLock();

            try
            {
                return string.Join(Eol.AsString(), Document.Lines.Select(ln => ln.Text));
            }
            finally
            {
                @lock.Release();
            }
        }

        public IEditorLock ObtainLock()
        {
            lockCount++;
            return editorLock;
        }

        public bool BeginUndoAction()
        {
            if (!undoGroup)
            {
                counter++;
                undoGroup = true;
                return true;
            }

            return false;
        }

        public void EndUndoAction() => undoGroup = false;

        internal void AddCommand(EditorCommand cmd) =>
            UndoStack.Push(new CommandInfo { Id = counter, Command = cmd });

        internal bool LastAtomicChange { get; set; }

        public bool Locked => lockCount > 0;

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

        public string FileName { get; }

        public Encoding Encoding { get; }
    }
}
