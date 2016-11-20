using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionResults;
using System.Windows.Forms;
using CodeBox.ComponentModel;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    public abstract class EditorCommand : ICommandComponent
    {
        public void Run(IExecutionContext ctx)
        {
            View = ctx as IEditorView;

            if (View == null)
                return;

            View.FirstEditLine = int.MaxValue;
            View.LastEditLine = 0;
            var lines = Document.Lines;
            var modify = ModifyContent;

            if (modify && (Buffer.ReadOnly || Buffer.Locked))
                return;
            
            var selCount = Buffer.Selections.Count;
            var qry = selCount == 1 ? null
                : Buffer.Selections.OrderByDescending(s => s.End > s.Start ? s.Start : s.End);
            var exp = None;
            var thisUndo = false;
            var lastSel = Buffer.Selections.Main;

            if (qry == null || SingleRun)
            {
                View.FirstEditLine = lastSel.GetFirstLine();
                View.LastEditLine = lastSel.GetLastLine();
                exp = Execute(lastSel);

                if (exp.Has(Modify) && (!Buffer.LastAtomicChange || !exp.Has(AtomicChange)))
                    thisUndo = Buffer.BeginUndoAction();

                if (exp.Has(Modify))
                    Buffer.AddCommand(this);

                if (exp.Has(RestoreCaret))
                    AttachCaret(lastSel.Caret);
            }
            else
            {
                thisUndo = Buffer.BeginUndoAction();
                var cc = 0;
                EditorCommand cmd = this;

                foreach (var sel in qry)
                {
                    var fel = sel.GetFirstLine();

                    if (fel < View.FirstEditLine)
                        View.FirstEditLine = fel;

                    var lel = sel.GetLastLine();

                    if (lel > View.LastEditLine)
                        View.LastEditLine = lel;

                    var e = cmd.Execute(sel);

                    exp |= e;

                    if (e.Has(Modify))
                        Buffer.AddCommand(cmd);

                    if (e.Has(RestoreCaret))
                        AttachCaret(sel.Caret);

                    lastSel = sel;

                    if (e.Has(Modify) && ++cc < selCount)
                    {
                        cmd = cmd.Clone();
                        cmd.View = View;
                    }
                }
            }

            if (thisUndo)
                Buffer.EndUndoAction();

            if (exp != None)
                DoAftermath(exp, Buffer.Selections.Count, lastSel.Caret);

            if (!exp.Has(IdleCaret))
                View.MatchBrackets.Match();

            if (exp.Has(AutocompleteKeep) && View.Autocomplete.WindowShown)
                View.Autocomplete.UpdateAutocomplete();
            else if (qry == null && exp.Has(AutocompleteShow))
                View.Autocomplete.ShowAutocomplete(lastSel.Caret);
            else if (!exp.Has(AutocompleteShow) && View.Autocomplete.WindowShown)
                View.Autocomplete.HideAutocomplete();

            Buffer.LastAtomicChange = qry == null && exp.Has(AtomicChange);
        }

        public virtual bool SingleRun => false;

        public virtual bool ModifyContent => false;

        protected abstract ActionResults Execute(Selection sel);

        public virtual ActionResults Undo(out Pos pos)
        {
            pos = Pos.Empty;
            return None;
        }

        public virtual ActionResults Redo(out Pos pos)
        {
            pos = Pos.Empty;
            return None;
        }

        protected void SetEditLines()
        {
            View.FirstEditLine = int.MaxValue;
            View.LastEditLine = 0;

            foreach (var s in Buffer.Selections)
            {
                var ln = s.GetFirstLine();

                if (ln < View.FirstEditLine)
                    View.FirstEditLine = ln;

                ln = s.GetLastLine();

                if (ln > View.LastEditLine)
                    View.LastEditLine = ln;
            }
        }

        protected void AttachCaret(Pos pos)
        {
            var line = Document.Lines[pos.Line];

            if (line.Length > pos.Col)
            {
                var ch = line.CharacterAt(pos.Col).WithCaret();
                line[pos.Col] = ch;
            }
            else
                line.TrailingCaret = true;
        }

        protected void SetCarets(int count, Pos pos)
        {
            var sels = Buffer.Selections;
            sels.Clear();

            for (var i = pos.Line; i < Document.Lines.Count; i++)
            {
                var line = Document.Lines[i];

                if (line.TrailingCaret)
                {
                    sels.Add(new Selection(new Pos(i, line.Length)));
                    line.TrailingCaret = false;
                    count--;

                    if (count == 0)
                        return;

                    continue;
                }

                for (var j = 0; j < line.Length; j++)
                {
                    var c = line[j];

                    if (c.HasCaret)
                    {
                        sels.Add(new Selection(new Pos(i, j)));
                        line[j] = c.WithoutCaret();
                        count--;

                        if (count == 0)
                            return;
                    }
                }
            }
        }

        protected void DoAftermath(ActionResults exp, int selCount, Pos caret)
        {
            var scrolled = false;

            if (exp.Has(Modify))
            {
                View.Scroll.InvalidateLines(
                    exp.Has(AtomicChange) ? InvalidateFlags.Atomic : InvalidateFlags.None);

                if (View.Scroll.ScrollPosition.Y + View.Info.TextHeight < -View.Scroll.ScrollBounds.Height)
                    exp |= Scroll;

                if (!exp.Has(ShallowChange))
                    View.Buffer.Edits++;

                if (!exp.Has(KeepRedo))
                    View.Buffer.RedoStack.Clear();
            }

            if (exp.Has(RestoreCaret))
                SetCarets(selCount, caret);

            if (exp.Has(Scroll))
            {
                View.Scroll.SuppressOnScroll = true;
                scrolled = View.Scroll.UpdateVisibleRectangle();
                View.Scroll.SuppressOnScroll = false;
            }

            if (scrolled || exp.Has(Modify))
                View.Styles.Restyle();

            if (!exp.Has(Silent))
                ((Editor)View).Redraw();

            if (exp.Has(Modify))
                View.Folding.RebuildFolding();

            if (exp.Has(LeaveEditor))
                View.Buffer.Selections.Truncate();
        }

        internal virtual EditorCommand Clone() => this;

        public IEditorView View { get; set; }

        protected DocumentBuffer Buffer => View.Buffer;

        protected Document Document => View.Buffer.Document;

        protected EditorSettings Settings => View.Settings;
    }
}
