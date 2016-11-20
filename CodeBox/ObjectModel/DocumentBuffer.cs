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

        internal DocumentBuffer(Document doc)
        {
            Document = doc;
            Selections = new SelectionList();
            UndoStack = new LimitedStack<CommandInfo>();
            RedoStack = new LimitedStack<CommandInfo>();
            Tips = new List<CallTip>();
            editorLock = new EditorLock(this);
        }

        internal string GetText() =>
            string.Join(Eol.AsString(), Document.Lines.Select(ln => ln.Text));

        public string[] GetLines() => Document.Lines.Select(ln => ln.Text).ToArray();

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
    }
}
