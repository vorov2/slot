using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;

namespace CodeBox
{
    public sealed class EditorContext
    {
        private readonly Editor editor;

        internal EditorContext(Editor editor)
        {
            this.editor = editor;
        }

        internal void ValidateCaret(Selection sel)
        {
            var caret = sel.Caret;
            var len = Document.Selections.TotalCount;

            if ((caret.Col < 0 || caret.Line < 0) ||
                (caret.Line == Document.Lines.Count - 1
                    && caret.Col > Document.Lines[caret.Line].Length))
                Document.Selections.Remove(sel);
            else if (len > 1)
            {
                for (var i = 0; i < len; i++)
                {
                    var s = Document.Selections[i];

                    if (s != sel && s.Caret.Line == caret.Line && s.Caret.Col == caret.Col)
                    {
                        Document.Selections.Remove(s);
                        len--;
                        i--;
                    }
                }
            }
        }

        public EditorSettings Settings
        {
            get { return editor.Settings; }
        }

        public Document Document
        {
            get { return editor.Document; }
        }

        public char Char { get; internal set; }

        public string String { get; internal set; }

        public int Edits { get; set; }

        public int StripesPerScreen
        {
            get { return Info.ClientHeight / Info.LineHeight; }
        }

        public Point Scroll
        {
            get { return editor.AutoScrollPosition; }
        }

        public bool Overtype
        {
            get { return editor.Overtype; }
        }

        public Eol Eol
        {
            get
            {
                return editor.Settings.Eol == Eol.Auto ?
                    editor.OriginalEol : editor.Settings.Eol;
            }
        }

        public EditorInfo Info
        {
            get { return editor.Info; }
        }

        internal Renderer Renderer
        {
            get { return editor.Renderer; }
        }
    }
}
