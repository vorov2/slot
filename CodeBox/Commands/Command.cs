using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionResults;
using System.Windows.Forms;

namespace CodeBox.Commands
{
    public abstract class EditorCommand : IEditorCommand
    {
        public void Run(IExecutionContext context)
        {
            var ctx = (IEditorContext)context;
            Context = ctx;
            ctx.FirstEditLine = int.MaxValue;
            ctx.LastEditLine = 0;
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
                ctx.FirstEditLine = lastSel.GetFirstLine();
                ctx.LastEditLine = lastSel.GetLastLine();
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
                IEditorCommand cmd = this;

                foreach (var sel in qry)
                {
                    var fel = sel.GetFirstLine();

                    if (fel < ctx.FirstEditLine)
                        ctx.FirstEditLine = fel;

                    var lel = sel.GetLastLine();

                    if (lel > ctx.LastEditLine)
                        ctx.LastEditLine = lel;

                    cmd.Context = ctx;
                    var e = cmd.Execute(sel);

                    exp |= e;

                    if (e.Has(Modify))
                        Buffer.AddCommand(cmd);

                    if (e.Has(RestoreCaret))
                        AttachCaret(sel.Caret);

                    lastSel = sel;

                    if (e.Has(Modify) && ++cc < selCount)
                        cmd = cmd.Clone();
                }
            }

            if (thisUndo)
                Buffer.EndUndoAction();

            if (exp != None)
                DoAftermath(exp, Buffer.Selections.Count, lastSel.Caret);

            if (!exp.Has(IdleCaret))
                ctx.MatchBrackets.Match();

            if (exp.Has(AutocompleteKeep) && ctx.Autocomplete.WindowShown)
                ctx.Autocomplete.UpdateAutocomplete();
            else if (qry == null && exp.Has(AutocompleteShow))
                ctx.Autocomplete.ShowAutocomplete(lastSel.Caret);
            else if (!exp.Has(AutocompleteShow) && ctx.Autocomplete.WindowShown)
                ctx.Autocomplete.HideAutocomplete();

            Buffer.LastAtomicChange = qry == null && exp.Has(AtomicChange);
        }

        public virtual bool SingleRun => false;

        public virtual bool ModifyContent => false;

        public abstract ActionResults Execute(Selection sel);

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
            Context.FirstEditLine = int.MaxValue;
            Context.LastEditLine = 0;

            foreach (var s in Buffer.Selections)
            {
                var ln = s.GetFirstLine();

                if (ln < Context.FirstEditLine)
                    Context.FirstEditLine = ln;

                ln = s.GetLastLine();

                if (ln > Context.LastEditLine)
                    Context.LastEditLine = ln;
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
                Context.Scroll.InvalidateLines(
                    exp.Has(AtomicChange) ? ScrollingManager.InvalidateFlags.Atomic
                    : ScrollingManager.InvalidateFlags.None);

                if (Context.Scroll.Y + Context.Info.TextHeight < -Context.Scroll.YMax)
                    exp |= Scroll;

                if (!exp.Has(ShallowChange))
                    Context.Buffer.Edits++;
            }

            if (exp.Has(RestoreCaret))
                SetCarets(selCount, caret);

            if (exp.Has(Scroll))
            {
                Context.Scroll.SuppressOnScroll = true;
                scrolled = Context.Scroll.UpdateVisibleRectangle();
                Context.Scroll.SuppressOnScroll = false;
            }

            if (scrolled || exp.Has(Modify))
                Context.Styles.Restyle();

            if (!exp.Has(Silent))
                ((Editor)Context).Redraw();

            if (exp.Has(Modify))
                Context.Folding.RebuildFolding();

            if (exp.Has(LeaveEditor))
                Context.Buffer.Selections.Truncate();
        }

        public virtual IEditorCommand Clone() => this;

        public IEditorContext Context { get; set; }

        protected DocumentBuffer Buffer => Context.Buffer;

        protected Document Document => Context.Buffer.Document;

        protected EditorSettings Settings => Context.Settings;
    }
}
