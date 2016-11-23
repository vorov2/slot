using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.Styling;
using System.Drawing;
using CodeBox.ObjectModel;
using CodeBox.Lexing;
using CodeBox.ComponentModel;
using CodeBox.Core.ComponentModel;
using CodeBox.Core;

namespace CodeBox.Styling
{
    public sealed class StyleManager
    {
        private readonly Editor editor;

        public StyleManager(Editor editor, StyleCollection styles)
        {
            this.editor = editor;
            Styles = styles;
        }

        public void ClearStyles(int line) => editor.Lines[line].AppliedStyles.Clear();

        public void StyleRange(int style, int line, int start, int end) =>
            editor.Lines[line].AppliedStyles.Add(new AppliedStyle(style, start, end));

        public void StyleRange(StandardStyle style, int line, int start, int end) =>
            editor.Lines[line].AppliedStyles.Add(new AppliedStyle((int)style, start, end));

        internal void Restyle()
        {
            if (editor.Lines.Count == 0)
                return;

            var fvl = editor.FirstEditLine < 0 ? editor.Scroll.FirstVisibleLine : editor.FirstEditLine;
            /*editor.FirstEditLine < editor.Scroll.FirstVisibleLine
                ? editor.FirstEditLine : editor.Scroll.FirstVisibleLine;*/
            var lvl = editor.Scroll.LastVisibleLine;
            var state = 0;

            while (fvl > -1 && (state = editor.Lines[fvl].State) != 0)
                fvl--;

            fvl = fvl < 0 ? 0 : fvl;
            lvl = lvl < fvl ? fvl : lvl;
            var range = new Range(new Pos(fvl, 0),
                new Pos(lvl, editor.Lines[lvl].Length - 1));
            RestyleRange(range);
        }

        internal void RestyleDocument()
        {
            var range = new Range(new Pos(0, 0), new Pos(editor.Lines.Count - 1, 0));
            RestyleRange(range);
        }

        private void RestyleRange(Range range)
        {
            if (range.End.Line < 0)
                return;

            if (StyleNeeded != null)
                StyleNeeded?.Invoke(this, new StyleNeededEventArgs(range));
            else if (Styler != null)
                Styler.Style(editor, range);
        }

        public event EventHandler<StyleNeededEventArgs> StyleNeeded;

        public IStylerComponent Styler { get; private set; }

        private Identifier _stylerKey;
        public Identifier StylerKey
        {
            get { return _stylerKey; }
            set
            {
                if (value != _stylerKey)
                {
                    _stylerKey = value;
                    Styler = ComponentCatalog.Instance.GetComponent(value) as IStylerComponent;
                }
            }
        }

        public StyleCollection Styles { get; set; }
    }
}
