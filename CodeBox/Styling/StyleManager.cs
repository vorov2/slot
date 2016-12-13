using System;
using System.Drawing;
using CodeBox.ObjectModel;
using CodeBox.ComponentModel;
using CodeBox.Core;
using CodeBox.Core.Themes;

namespace CodeBox.Styling
{
    public sealed class StyleManager
    {
        private static readonly Identifier defaultStyler = (Identifier)"styler.default";
        private readonly Editor editor;

        public StyleManager(Editor editor)
        {
            this.editor = editor;
        }

        public void ClearStyles(int line) => editor.Lines[line].AppliedStyles.Clear();

        public void StyleRange(StandardStyle style, int line, int start, int end) =>
            editor.Lines[line].AppliedStyles.Add(new AppliedStyle(style, start, end));

        internal void Restyle()
        {
            if (editor.Lines.Count == 0)
                return;

            var fvl = editor.FirstEditLine < 0 || editor.FirstEditLine >= editor.Lines.Count
                ? editor.Scroll.FirstVisibleLine : editor.FirstEditLine;
            var lvl = editor.LastEditLine > editor.Scroll.LastVisibleLine ? editor.LastEditLine
                : editor.Scroll.LastVisibleLine;
            var state = 0;

            while (fvl > -1 && (state = editor.Lines[fvl].State) != 0)
                fvl--;

            fvl = fvl < 0 ? 0 : fvl;
            lvl = lvl < fvl || lvl >= editor.Lines.Count ? fvl : lvl;
            var range = new Range(new Pos(fvl, 0),
                new Pos(lvl, editor.Lines[lvl].Length - 1));
            RestyleRange(range);
        }

        public void RestyleDocument()
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
            else
            {
                var grm = App.Ext.Grammars().GetGrammar(editor.Buffer.GrammarKey);
                var styler = grm == null ? null
                    : App.Catalog<IStylerComponent>().GetComponent(grm.StylerKey ?? defaultStyler);
                if (styler != null)
                    styler.Style(editor, range);
            }
        }

        public event EventHandler<StyleNeededEventArgs> StyleNeeded;
    }
}
