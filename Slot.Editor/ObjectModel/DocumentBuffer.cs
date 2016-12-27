using Slot.Editor.Commands;
using Slot.Editor.Styling;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Slot.Editor.CallTips;
using System.Text;
using Slot.Core.ViewModel;
using System.IO;
using System.Drawing;
using Slot.Core.Output;
using Slot.Editor.Folding;
using Slot.Core;

namespace Slot.Editor.ObjectModel
{
    public class DocumentBuffer : IBuffer
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

        public DocumentBuffer(Document doc, FileInfo file, Encoding encoding, Guid id)
        {
            Id = id;
            Document = doc;
            Selections = new SelectionList();
            UndoStack = new LimitedStack<CommandInfo>();
            RedoStack = new LimitedStack<CommandInfo>();
            Tips = new List<CallTip>();
            Encoding = encoding;
            File = file;
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

        public string GetContents() => GetText();

        public void Truncate(string text = "")
        {
            var @lock = ObtainLock();

            try
            {
                Document.Lines.Clear();
                //Document.Lines.Add(Line.FromString(text));
                Document.Lines.AddRange(text.MakeCharacters().MakeLines().Select(cz => new Line(cz)));
                Selections.Set(new Pos(0, 0));
                Edits = 0;

                foreach (var v in Views)
                    v.AttachBuffer(this);
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

        void IBuffer.SerializeState(Stream stream)
        {
            while (Views.Count > 0)
                Views[0].DetachBuffer();

            try
            {
                DocumentBufferSerializer.Serialize(this, stream);
            }
            catch
            {
                App.Ext.Log($"Unable to serialize buffer state: {File}", EntryType.Error);
            }
        }

        void IBuffer.DeserializeState(Stream stream)
        {
            try
            {
                DocumentBufferSerializer.Deserialize(this, stream);
            }
            catch
            {
                App.Ext.Log($"Unable to deserialize buffer state: {File}", EntryType.Error);
            }
        }

        public void ClearDirtyFlag()
        {
            Edits = 0;
            LastAtomicChange = false;
            RequestRedraw();
        }

        internal readonly List<EditorControl> Views = new List<EditorControl>();
        public void RequestRedraw()
        {
            if (!Locked)
                foreach (var e in Views)
                    e.Redraw();
        }

        internal void ResetInvalidation()
        {
            if (!Locked)
                foreach (var v in Views)
                {
                    v.Scroll.ScrollPosition = default(Point);
                    v.Scroll.InvalidateLines(InvalidateFlags.Force);
                    v.Redraw();
                }
        }

        internal void InvalidateLines()
        {
            if (!Locked)
                foreach (var e in Views)
                    e.Scroll.InvalidateLines();
        }

        internal void ScrollToCaret()
        {
            if (!Locked)
                foreach (var e in Views)
                    e.Scroll.UpdateVisibleRectangle();
        }

        internal void UpdateScrollInfo(EditorControl view)
        {
            foreach (var e in Views)
            {
                if (e != view)
                {
                    e.Scroll.InvalidateLines();
                    if (-e.Scroll.ScrollPosition.Y > e.Scroll.ScrollBounds.Height)
                        e.Scroll.ScrollPosition = new Point(0, 0);
                }
            }
        }

        public Guid Id { get; }

        public Point ScrollPosition { get; internal set; }

        internal bool LastAtomicChange { get; set; }

        public bool Locked => lockCount > 0;

        internal List<CallTip> Tips { get; }

        internal LimitedStack<CommandInfo> UndoStack { get; }

        internal LimitedStack<CommandInfo> RedoStack { get; }

        public Document Document { get; internal set; }

        public SelectionList Selections { get; }

        internal int Edits { get; set; }

        private bool _overtype;
        public bool Overtype
        {
            get { return _overtype; }
            set
            {
                if (value != _overtype)
                {
                    _overtype = value;

                    if (!Locked)
                        foreach (var v in Views)
                        {
                            v.CaretRenderer.BlockCaret = value;
                            v.Redraw();
                        }
                }
            }
        }

        private bool? _wordWrap;
        public bool? WordWrap
        {
            get { return _wordWrap; }
            set
            {
                _wordWrap = value;
                ResetInvalidation();
            }
        }

        private int? _wordWrapColumn;
        public int? WordWrapColumn
        {
            get { return _wordWrapColumn; }
            set
            {
                _wordWrapColumn = value;
                ResetInvalidation();
            }
        }

        public bool? UseTabs { get; set; }

        public int? IndentSize { get; set; }

        private bool? _showEol;
        public bool? ShowEol
        {
            get { return _showEol; }
            set
            {
                _showEol = value;
                RequestRedraw();
            }
        }

        private ShowWhitespace? _showWhitespace;
        public ShowWhitespace? ShowWhitespace
        {
            get { return _showWhitespace; }
            set
            {
                _showWhitespace = value;
                RequestRedraw();
            }
        }

        private bool? _showLineLength;
        public bool? ShowLineLength
        {
            get { return _showLineLength; }
            set
            {
                _showLineLength = value;
                RequestRedraw();
            }
        }

        private bool? _currentLineIndicator;
        public bool? CurrentLineIndicator
        {
            get { return _currentLineIndicator; }
            set
            {
                _currentLineIndicator = value;
                RequestRedraw();
            }
        }

        private bool? _readOnly;
        public bool? ReadOnly
        {
            get { return _readOnly; }
            set
            {
                if (value != _readOnly)
                {
                    _readOnly = value;
                    RequestRedraw();
                }
            }
        }

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

        public Identifier GrammarKey { get; set; }

        public FileInfo File { get; set; }

        public Encoding Encoding { get; set; }

        public DateTime LastAccess { get; set; }

        public Identifier Mode
        {
            get { return GrammarKey; }
            set
            {
                GrammarKey = value;

                if (!Locked)
                {
                    Views.First().Styles.RestyleDocument();
                    RequestRedraw();
                }
            }
        }

        public BufferDisplayFlags Flags { get; set; }
    }

    internal sealed class DocumentBufferSerializer
    {
        private const int BIN_VERSION = 3;

        public static void Deserialize(DocumentBuffer buffer, Stream stream)
        {
            var sr = new BinaryReader(stream, Encoding.UTF8);

            if (sr.ReadInt32() != BIN_VERSION)
            {
                App.Ext.Log($"Invalid version of document state. Expected: {BIN_VERSION}.", EntryType.Error);
                return;
            }

            var @lock = buffer.ObtainLock();

            try
            {
                //Main
                buffer.ReadOnly = sr.ReadNullableBool();
                buffer.Encoding = Encoding.GetEncoding(sr.ReadInt32());
                buffer.File = new FileInfo(sr.ReadString());
                buffer.GrammarKey = (Identifier)sr.ReadString();
                buffer.Flags = (BufferDisplayFlags)sr.ReadInt32();

                //ScrollPosition
                buffer.ScrollPosition = new Point(sr.ReadInt32(), sr.ReadInt32());

                //Selections
                var count = sr.ReadInt32();
                buffer.Selections.Clear();

                for (var i = 0; i < count; i++)
                {
                    var sel = new Selection(
                        new Pos(sr.ReadInt32(), sr.ReadInt32()),
                        new Pos(sr.ReadInt32(), sr.ReadInt32())
                        );
                    buffer.Selections.AddFast(sel);
                }

                //Collapsed lines
                count = sr.ReadInt32();

                for (var i = 0; i < count; i++)
                    buffer.Document.Lines[sr.ReadInt32()].Folding |= FoldingStates.Invisible;

                //Properties
                buffer.WordWrap = sr.ReadNullableBool();
                buffer.Overtype = sr.ReadBoolean();
                buffer.UseTabs = sr.ReadNullableBool();
                buffer.IndentSize = sr.ReadNullableInt32();
                buffer.ShowEol = sr.ReadNullableBool();
                buffer.ShowWhitespace = sr.ReadNullableShowWhitespace();
                buffer.ShowLineLength = sr.ReadNullableBool();
                buffer.CurrentLineIndicator = sr.ReadNullableBool();
                buffer.Eol = (Eol)sr.ReadInt32();
            }
            finally
            {
                @lock.Release();
            }
        }

        public static void Serialize(DocumentBuffer buffer, Stream stream)
        {
            var sw = new BinaryWriter(stream, Encoding.UTF8);
            sw.Write(BIN_VERSION);

            //Main
            sw.Write(buffer.ReadOnly);
            sw.Write(buffer.Encoding.CodePage);
            sw.Write(buffer.File.FullName);
            sw.Write(buffer.GrammarKey.ToString());
            sw.Write((int)buffer.Flags);

            //ScrollPosition
            sw.Write(buffer.ScrollPosition.X);
            sw.Write(buffer.ScrollPosition.Y);

            //Selections
            sw.Write(buffer.Selections.Count);

            foreach (var sel in buffer.Selections)
            {
                sw.Write(sel.Start.Line);
                sw.Write(sel.Start.Col);
                sw.Write(sel.End.Line);
                sw.Write(sel.End.Col);
            }

            //Collapsed lines
            var lines = new List<int>();

            for (var i = 0; i < buffer.Document.Lines.Count; i++)
            {
                var ln = buffer.Document.Lines[i];
                if (ln.Folding.Has(FoldingStates.Invisible))
                    lines.Add(i);
            }

            sw.Write(lines.Count);

            foreach (var i in lines)
                sw.Write(i);

            //Properties
            sw.Write(buffer.WordWrap);
            sw.Write(buffer.Overtype);
            sw.Write(buffer.UseTabs);
            sw.Write(buffer.IndentSize);
            sw.Write(buffer.ShowEol);
            sw.Write(buffer.ShowWhitespace);
            sw.Write(buffer.ShowLineLength);
            sw.Write(buffer.CurrentLineIndicator);
            sw.Write((int)buffer.Eol);
        }
    }

    internal static class BinaryReaderExtensions
    {
        public static bool? ReadNullableBool(this BinaryReader sr)
        {
            if (!sr.ReadBoolean())
                return null;

            return sr.ReadBoolean();
        }

        public static int? ReadNullableInt32(this BinaryReader sr)
        {
            if (!sr.ReadBoolean())
                return null;

            return sr.ReadInt32();
        }

        public static ShowWhitespace? ReadNullableShowWhitespace(this BinaryReader sr)
        {
            if (!sr.ReadBoolean())
                return null;

            return (ShowWhitespace)sr.ReadInt32();
        }
    }

    internal static class BinaryWriterExtensions
    {
        public static void Write(this BinaryWriter sw, bool? value)
        {
            sw.Write(value != null);
            sw.Write(value != null ? value.Value : false);
        }

        public static void Write(this BinaryWriter sw, int? value)
        {
            sw.Write(value != null);
            sw.Write(value != null ? value.Value : 0);
        }

        public static void Write(this BinaryWriter sw, ShowWhitespace? value)
        {
            sw.Write(value != null);
            sw.Write(value != null ? (int)value.Value : 0);
        }
    }
}
