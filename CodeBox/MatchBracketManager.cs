using CodeBox.ObjectModel;
using CodeBox.Styling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CodeBox.Affinity;

namespace CodeBox
{
    internal sealed class MatchBracketManager
    {
        private readonly Editor editor;
        private bool markedParent;
        private int edits;
        private Pos lastPos;
        private Tuple<int, AppliedStyle> match1;
        private Tuple<int, AppliedStyle> match2;

        public MatchBracketManager(Editor editor)
        {
            this.editor = editor;
        }

        public void Match()
        {
            InternalMatch();
        }

        internal bool IsBracketStyle(Line line, int col)
        {
            foreach (var a in line.AppliedStyles)
                if (col >= a.Start && col <= a.End)
                    return a.StyleId == (int)StandardStyle.Default
                        || a.StyleId == (int)StandardStyle.MatchedBracket
                        || a.StyleId == (int)StandardStyle.Bracket;

            return true;
        }

        private void InternalMatch()
        {
            if (editor.Buffer.Edits == edits && lastPos == editor.Buffer.Selections.Main.Caret)
            {
                if (match1 != null && match2 != null)
                {
                    editor.Lines[match1.Item1].AppliedStyles.Add(match1.Item2);
                    editor.Lines[match2.Item1].AppliedStyles.Add(match2.Item2);
                }

                return;
            }

            match1 = null;
            match2 = null;
            edits = editor.Buffer.Edits;
            lastPos = editor.Buffer.Selections.Main.Caret;

            if (markedParent)
            {
                editor.Styles.Restyle();
                markedParent = false;
            }

            Console.WriteLine("Match braket");
            foreach (var sel in editor.Buffer.Selections)
            {
                var ln = editor.Lines[sel.Caret.Line];
                var grm1 = editor.AffinityManager.GetAffinity(sel.Caret);
                var grm2 = editor.AffinityManager.GetAffinity(new Pos(sel.Caret.Line, sel.Caret.Col - 1));
                var bracketsForward = grm1.GetBracketSymbols(editor, sel.Caret);
                var bracketsBackward = grm2.GetBracketSymbols(editor, new Pos(sel.Caret.Line, sel.Caret.Col - 1));
                var pi = -1;

                if (sel.Caret.Col < ln.Length
                    && Even(pi = bracketsForward.IndexOf(ln[sel.Caret.Col].Char))
                    && IsBracketStyle(ln, sel.Caret.Col))
                {
                    var m = TraverseForward(grm1, bracketsForward, pi, sel);
                    if (!markedParent)
                        markedParent = m;
                }
                else if (sel.Caret.Col > 0
                    && sel.Caret.Col <= ln.Length
                    && Odd(pi = bracketsBackward.IndexOf(ln[sel.Caret.Col - 1].Char))
                    && IsBracketStyle(ln, sel.Caret.Col))
                {
                    var m = TraverseBackward(grm2, bracketsBackward, pi, sel);
                    if (!markedParent)
                        markedParent = m;
                }
            }
        }

        private bool TraverseForward(IDocumentAffinity aff, string brackets, int pi, Selection sel)
        {
            var cc = 0;

            for (var lni = sel.Caret.Line; lni < editor.Lines.Count; lni++)
            {
                var line = editor.Lines[lni];
                var ist = lni == sel.Caret.Line ? sel.Caret.Col + 1 : 0;
                var grm = editor.AffinityManager.GetAffinity(new Pos(lni, ist));

                if (grm != aff)
                    continue;

                for (var i = ist; i < line.Length; i++)
                {
                    if (line[i].Char == brackets[pi] && IsBracketStyle(line, i))
                        cc++;
                    else if (line[i].Char == brackets[pi + 1] && IsBracketStyle(line, i))
                    {
                        if (cc > 0)
                            cc--;
                        else
                        {
                            Style(sel.Caret.Line, sel.Caret.Col);
                            Style(lni, i);
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private bool TraverseBackward(IDocumentAffinity aff, string brackets, int pi, Selection sel)
        {
            var cc = 0;

            for (var lni = sel.Caret.Col > 2 ? sel.Caret.Line : sel.Caret.Line - 1; lni > -1; lni--)
            {
                var line = editor.Lines[lni];
                var ist1 = lni == sel.Caret.Line ? sel.Caret.Col - 1 : line.Length - 1;
                var ist2 = lni == sel.Caret.Line ? sel.Caret.Col - 2 : line.Length - 1;
                var grm = editor.AffinityManager.GetAffinity(new Pos(lni, ist1));

                if (grm != aff)
                    continue;

                for (var i = ist2; i > -1; i--)
                {
                    if (line[i].Char == brackets[pi] && IsBracketStyle(line, i))
                        cc++;
                    else if (line[i].Char == brackets[pi - 1] && IsBracketStyle(line, i))
                    {
                        if (cc > 0)
                            cc--;
                        else
                        {
                            Style(sel.Caret.Line, sel.Caret.Col - 1);
                            Style(lni, i);
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private void Style(int line, int col)
        {
            var tup = Tuple.Create(line, new AppliedStyle((int)StandardStyle.MatchedBracket, col, col));
            editor.Lines[line].AppliedStyles.Add(tup.Item2);

            if (match1 == null)
                match1 = tup;
            else
                match2 = tup;
        }

        private bool Even(int num)
        {
            return num != -1 && num % 2 == 0;
        }

        private bool Odd(int num)
        {
            return num != -1 && num % 2 != 0;
        }
    }
}
