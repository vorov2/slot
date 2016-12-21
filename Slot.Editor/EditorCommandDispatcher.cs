using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Slot.Core;
using Slot.Core.CommandModel;
using Slot.Core.ComponentModel;
using Slot.Core.ViewModel;
using Slot.Editor.Commands;
using Slot.Editor.ObjectModel;
using static Slot.Editor.Commands.ActionResults;

namespace Slot.Editor
{
    [Export(typeof(ICommandDispatcher))]
    [ComponentData(Name)]
    public sealed class EditorCommandDispatcher : ICommandDispatcher
    {
        public const string Name = "editor";

        [ImportMany]
        private IEnumerable<Lazy<EditorCommand, IComponentMetadata>> commands = null;
        private Dictionary<Identifier, EditorCommand> commandMap = new Dictionary<Identifier, EditorCommand>();

        private EditorCommand GetCommand(Identifier key)
        {
            EditorCommand ret;

            if (!commandMap.TryGetValue(key, out ret))
            {
                var strKey = key.ToString();
                var cmd = commands.FirstOrDefault(c => c.Metadata.Key == strKey);

                if (cmd != null)
                {
                    commandMap.Add(key, cmd.Value);
                    ret = cmd.Value;
                }
                else
                    return null;
            }

            return ret;
        }

        public bool Execute(IView view, Identifier commandKey, params object[] args)
        {
            var editor = view as EditorControl;

            if (editor == null)
                return false;

            editor.FirstEditLine = editor.LimitedMode ? 0 : int.MaxValue;
            editor.LastEditLine = 0;
            var cmd = GetCommand(commandKey);

            if (cmd == null || editor.LimitedMode && !cmd.SupportLimitedMode)
                return false;

            var modify = cmd.ModifyContent;

            if (modify && (editor.Buffer.ReadOnly || editor.Buffer.Locked))
                return false;

            var lines = editor.Lines;
            var selCount = editor.Buffer.Selections.Count;
            var qry = selCount == 1 || editor.LimitedMode ? null
                : editor.Buffer.Selections.OrderByDescending(s => s.End > s.Start ? s.Start : s.End);
            var exp = None;
            var thisUndo = false;
            var lastSel = editor.Buffer.Selections.Main;

            if (qry == null || cmd.SingleRun)
            {
                var fel = lastSel.GetFirstLine();
                var lel = lastSel.GetLastLine();
                cmd = cmd.Clone();
                cmd.View = editor;
                exp = cmd.Execute(lastSel, args);

                if (exp.Has(Modify) && (!editor.Buffer.LastAtomicChange || !exp.Has(AtomicChange))
                    && !editor.LimitedMode)
                    thisUndo = editor.Buffer.BeginUndoAction();

                if (exp.Has(Modify) && !editor.LimitedMode)
                {
                    editor.Buffer.AddCommand(cmd);
                    editor.FirstEditLine = fel;
                    editor.LastEditLine = lel;
                    lastSel.SetToRestore(0);

                    if (exp.Has(AtomicChange))
                        cmd.GroupUndo = true;
                }

                if (exp.Has(RestoreCaret))
                    AttachCaret(editor, lastSel.Caret);
            }
            else
            {
                thisUndo = editor.Buffer.BeginUndoAction();

                foreach (var sel in qry)
                {
                    var fel = sel.GetFirstLine();
                    var lel = sel.GetLastLine();
                    cmd = cmd.Clone();
                    cmd.View = editor;
                    var e = cmd.Execute(sel, args);
                    exp |= e;

                    if (e.Has(Modify))
                    {
                        editor.Buffer.AddCommand(cmd);
                        if (fel < editor.FirstEditLine)
                            editor.FirstEditLine = fel;
                        if (lel > editor.LastEditLine)
                            editor.LastEditLine = lel;
                        sel.SetToRestore(0);
                    }

                    if (e.Has(RestoreCaret))
                        AttachCaret(editor, sel.Caret);

                    lastSel = sel;
                }
            }

            if (thisUndo)
                editor.Buffer.EndUndoAction();

            //Console.WriteLine($"FirstEditLine: {editor.FirstEditLine}; LastEditLine: {editor.LastEditLine}");

            if (exp != None)
                DoAftermath(editor, exp, editor.Buffer.Selections.Count, lastSel.Caret, thisUndo ? 1 : 0);

            if (!exp.Has(IdleCaret) && !editor.LimitedMode)
            {
                editor.MatchBrackets.Match();
                editor.MatchWords.RequestMatch();
            }

            if (!editor.LimitedMode)
            {
                if (exp.Has(AutocompleteKeep) && editor.Autocomplete.WindowShown)
                    editor.Autocomplete.UpdateAutocomplete();
                else if (qry == null && exp.Has(AutocompleteShow))
                    editor.Autocomplete.ShowAutocomplete(lastSel.Caret);
                else if (!exp.Has(AutocompleteShow) && editor.Autocomplete.WindowShown)
                    editor.Autocomplete.HideAutocomplete();
            }

            editor.Buffer.LastAtomicChange = qry == null && exp.Has(AtomicChange);

            if (exp.Has(NeedUndo))
                Undo(editor);
            else if (exp.Has(NeedRedo))
                Redo(editor);

            return true;
        }

        private void SetEditLines(EditorControl editor)
        {
            foreach (var s in editor.Buffer.Selections)
            {
                var ln = s.GetFirstLine();

                if (ln < editor.FirstEditLine)
                    editor.FirstEditLine = ln;

                ln = s.GetLastLine();

                if (ln > editor.LastEditLine)
                    editor.LastEditLine = ln;
            }
        }

        private void AttachCaret(EditorControl editor, Pos pos)
        {
            var line = editor.Lines[pos.Line];

            if (line.Length > pos.Col)
            {
                var ch = line.CharacterAt(pos.Col).WithCaret();
                line[pos.Col] = ch;
            }
            else
                line.TrailingCaret = true;
        }

        private void SetCarets(EditorControl editor, int count, Pos pos)
        {
            var sels = editor.Buffer.Selections;
            sels.Clear();

            for (var i = pos.Line; i < editor.Lines.Count; i++)
            {
                var line = editor.Lines[i];

                if (line.TrailingCaret)
                {
                    sels.AddFast(new Selection(new Pos(i, line.Length)));
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
                        sels.AddFast(new Selection(new Pos(i, j)));
                        line[j] = c.WithoutCaret();
                        count--;

                        if (count == 0)
                            return;
                    }
                }
            }
        }

        private void DoAftermath(EditorControl editor, ActionResults exp, int selCount, Pos caret, int edit = 0)
        {
            var scrolled = false;

            if (exp.Has(Modify))
            {
                editor.Scroll.InvalidateLines(
                    exp.Has(AtomicChange) ? InvalidateFlags.Atomic : InvalidateFlags.None);

                if (editor.Scroll.ScrollPosition.Y + editor.Info.TextHeight < -editor.Scroll.ScrollBounds.Height)
                    exp |= Scroll;

                if (!exp.Has(ShallowChange))
                    editor.Buffer.Edits += edit;

                if (!exp.Has(KeepRedo))
                    editor.Buffer.RedoStack.Clear();

                if (editor.HasContentModified)
                    editor.OnContentModified();
            }

            if (exp.Has(SetEditRange))
            {
                editor.FirstEditLine = int.MaxValue;
                editor.LastEditLine = 0;
                SetEditLines(editor);
            }

            if (exp.Has(RestoreCaret))
                SetCarets(editor, selCount, caret);

            if (exp.Has(SetEditRange))
                SetEditLines(editor);

            if (exp.Has(Scroll))
            {
                editor.Scroll.SuppressOnScroll = true;
                scrolled = editor.Scroll.UpdateVisibleRectangle();
                editor.Scroll.SuppressOnScroll = false;
            }

            if (exp.Has(UpdateScrollInfo))
                editor.Buffer.UpdateScrollInfo(editor);

            if (/*scrolled ||*/ exp.Has(Modify))
            {
                editor.Styles.Restyle();
                editor.Search.RequestSearch();
            }

            if (!exp.Has(Silent))
                editor.Buffer.RequestRedraw();

            if (exp.Has(Modify))
                editor.Folding.RebuildFolding();

            if (exp.Has(LeaveEditor))
                editor.Buffer.Selections.Truncate();
        }

        private void Undo(EditorControl editor)
        {
            if (editor.Buffer.UndoStack.Count > 0)
            {
                Pos pos;
                int count;
                var exp = Undo(editor, editor.Buffer.UndoStack.Peek().Id, out count, out pos);
                DoAftermath(editor, exp | KeepRedo | SetEditRange, count, pos, -1);
            }
        }

        private ActionResults Undo(EditorControl editor, int id, out int count, out Pos pos)
        {
            var exp = default(ActionResults);
            pos = Pos.Empty;
            count = 0;
            var posRestore = Pos.Empty;

            while (editor.Buffer.UndoStack.Count > 0)
            {
                var cmd = editor.Buffer.UndoStack.Peek();

                if (cmd.Id == id)
                {
                    cmd.Command.View = editor;
                    Pos p;
                    var e = cmd.Command.Undo(out p);

                    if (pos.IsEmpty || p < pos)
                        pos = p;

                    exp |= e;

                    if (e.Has(RestoreCaret))
                    {
                        if (!cmd.Command.GroupUndo)
                            AttachCaret(editor, p);
                        else
                            posRestore = p;
                    }

                    editor.Buffer.RedoStack.Push(editor.Buffer.UndoStack.Pop());
                    count++;
                }
                else
                    break;
            }

            if (!posRestore.IsEmpty)
                AttachCaret(editor, posRestore);

            return exp;
        }

        private void Redo(EditorControl editor)
        {
            if (editor.Buffer.RedoStack.Count > 0)
            {
                Pos pos;
                int count;
                var exp = Redo(editor, editor.Buffer.RedoStack.Peek().Id, out count, out pos);
                DoAftermath(editor, exp | KeepRedo | SetEditRange, count, pos, 1);
            }
        }

        private ActionResults Redo(EditorControl editor, int id, out int count, out Pos pos)
        {
            var exp = default(ActionResults);
            pos = Pos.Empty;
            count = 0;
            var posRestore = Pos.Empty;

            while (editor.Buffer.RedoStack.Count > 0)
            {
                var cmd = editor.Buffer.RedoStack.Peek();

                if (cmd.Id == id)
                {
                    cmd.Command.View = editor;
                    Pos p;
                    var e = cmd.Command.Redo(out p);
                    pos = p;
                    exp |= e;

                    if (e.Has(RestoreCaret))
                    {
                        if (!cmd.Command.GroupUndo)
                            AttachCaret(editor, p);
                        else
                            posRestore = p;
                    }

                    editor.Buffer.UndoStack.Push(editor.Buffer.RedoStack.Pop());
                    count++;
                }
                else
                    break;
            }

            if (!posRestore.IsEmpty)
                AttachCaret(editor, posRestore);

            return exp;
        }
    }
}
