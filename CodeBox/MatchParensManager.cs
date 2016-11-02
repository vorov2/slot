using CodeBox.ObjectModel;
using CodeBox.Styling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox
{
    internal sealed class MatchParensManager
    {
        private readonly Editor editor;
        private const string START_PARENS = "([{";
        private const string END_PARENS = ")]}";
        private bool markedParent;

        public MatchParensManager(Editor editor)
        {
            this.editor = editor;
        }

        public void Match()
        {
            if (markedParent)
            {
                editor.Styles.Restyle();
                markedParent = false;
            }

            foreach (var sel in editor.Buffer.Selections)
            {
                var ln = editor.Lines[sel.Caret.Line];
                var pi = -1;

                if (sel.Caret.Col < ln.Length
                    && (pi = START_PARENS.IndexOf(ln[sel.Caret.Col].Char)) != -1
                    && ln.IsDefaultStyle(sel.Caret.Col))
                {
                    var m = TraverseForward(pi, sel);
                    if (!markedParent)
                        markedParent = m;
                }
                else if (sel.Caret.Col > 0
                    && (pi = END_PARENS.IndexOf(ln[sel.Caret.Col - 1].Char)) != -1
                    && ln.IsDefaultStyle(sel.Caret.Col))
                {
                    var m = TraverseBackward(pi, sel);
                    if (!markedParent)
                        markedParent = m;
                }
            }
        }

        private bool TraverseForward(int pi, Selection sel)
        {
            var cc = 0;

            for (var lni = sel.Caret.Line; lni < editor.Lines.Count; lni++)
            {
                var line = editor.Lines[lni];

                for (var i = lni == sel.Caret.Line ? sel.Caret.Col + 1 : 0; i < line.Length; i++)
                {
                    if (!line.IsDefaultStyle(i))
                        continue;

                    if (line[i].Char == START_PARENS[pi])
                        cc++;
                    else if (line[i].Char == END_PARENS[pi])
                    {
                        if (cc > 0)
                            cc--;
                        else
                        {
                            editor.Styles.StyleRange((int)StandardStyle.MatchBrace, sel.Caret.Line, sel.Caret.Col, sel.Caret.Col);
                            editor.Styles.StyleRange((int)StandardStyle.MatchBrace, lni, i, i);
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private bool TraverseBackward(int pi, Selection sel)
        {
            var cc = 0;

            for (var lni = sel.Caret.Col > 2 ? sel.Caret.Line : sel.Caret.Line - 1; lni > -1; lni--)
            {
                var line = editor.Lines[lni];

                for (var i = lni == sel.Caret.Line ? sel.Caret.Col - 2 : line.Length - 1; i > -1; i--)
                {
                    if (!line.IsDefaultStyle(i))
                        continue;

                    if (line[i].Char == END_PARENS[pi])
                        cc++;
                    else if (line[i].Char == START_PARENS[pi])
                    {
                        if (cc > 0)
                            cc--;
                        else
                        {
                            editor.Styles.StyleRange((int)StandardStyle.MatchBrace, sel.Caret.Line, sel.Caret.Col - 1, sel.Caret.Col - 1);
                            editor.Styles.StyleRange((int)StandardStyle.MatchBrace, lni, i, i);
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
