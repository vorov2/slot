using CodeBox.Commands;
using CodeBox.ObjectModel;
using CodeBox.Styling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeBox
{
    public sealed class MatchWordManager
    {
        private readonly Editor editor;
        private readonly List<Tuple<int,AppliedStyle>> finds = new List<Tuple<int,AppliedStyle>>();
        private string lastWord;
        private Pos requestCaret = Pos.Empty;
        private DateTime requestTime;

        public MatchWordManager(Editor editor)
        {
            this.editor = editor;
            Application.Idle += (o, e) => Match();
        }

        public void RequestMatch()
        {
            requestCaret = editor.Buffer.Selections.Main.Caret;
            requestTime = DateTime.Now;
        }

        private void Match()
        {
            var caret = editor.Buffer.Selections.Main.Caret;
            var range = SelectWordCommand.SelectWord(editor, caret, SelectWordCommand.Strategy.Word);
            var txt = range != null ? CopyCommand.GetTextRange(editor, range) : null;

            if (txt == lastWord && finds.Count > 0)
                return;

            var needRedraw = false;

            foreach (var f in finds)
            {
                editor.Lines[f.Item1].AppliedStyles.Remove(f.Item2);
                needRedraw = true;
            }

            finds.Clear();

            if (caret != requestCaret || (DateTime.Now - requestTime).TotalMilliseconds < 500)
            {
                if (needRedraw)
                    editor.Redraw();
                return;
            }

            lastWord = txt;

            if (range == null)
            {
                if (needRedraw)
                    editor.Redraw();
                return;
            }

            var grmId = editor.AffinityManager.GetAffinityId(caret);
            var grm = editor.GrammarManager.GetGrammar(grmId);
            var seps = (" \t" + (grm.NonWordSymbols ?? editor.Settings.NonWordSymbols)).ToCharArray();
            var regex = new Regex("\\b" + Regex.Escape(txt) + "\\b");

            for (var i = 0; i < editor.Lines.Count; i++)
            {
                var line = editor.Lines[i];
                var ln = line.Text;

                foreach (Match m in regex.Matches(ln))
                    if (m.Success && editor.AffinityManager.GetAffinityId(i, m.Index) == grmId)
                    {
                        var aps = new AppliedStyle((int)StandardStyle.MatchedWord, m.Index, m.Index + m.Length - 1);
                        line.AppliedStyles.Add(aps);
                        finds.Add(Tuple.Create(i, aps));
                    }
            }

            if (finds.Count == 1)
            {
                editor.Lines[finds[0].Item1].AppliedStyles.Remove(finds[0].Item2);
                finds.Clear();
            }

            if (finds.Count > 0 || needRedraw)
                editor.Redraw();

            requestCaret = Pos.Empty;
        }
    }
}
