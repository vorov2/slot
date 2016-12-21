using System;
using Slot.Editor.ObjectModel;
using Slot.Editor.Affinity;
using Slot.Editor.ComponentModel;
using System.ComponentModel.Composition;
using Slot.Core.ComponentModel;

namespace Slot.Editor.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.wordright")]
    public sealed class WordRightCommand : CaretCommand
    {
        protected override Pos GetPosition(Selection sel)
        {
            var pos = WordRight(View, sel);
            sel.SetToRestore(Document.Lines[pos.Line].GetStripeCol(pos.Col));
            System.Diagnostics.Debug.WriteLine(sel.RestoreCaretCol);
            return pos;
        }

        internal static Pos WordRight(EditorControl ctx, Selection sel)
        {
            var caret = sel.Caret;
            var line = ctx.Buffer.Document.Lines[caret.Line];

            if (caret.Col < line.Length - 1)
            {
                var seps = ctx.AffinityManager.GetAffinity(caret).GetNonWordSymbols(ctx);
                var c = line.CharAt(caret.Col);
                var strat = SelectWordCommand.GetStrategy(seps, c);
                var pos = SelectWordCommand.FindBoundRight(seps, line, caret.Col, strat);
                return new Pos(caret.Line, pos);
            }
            else
                return RightCommand.MoveRight(ctx, sel);
        }

        internal override bool SupportLimitedMode => true;
    }
}
