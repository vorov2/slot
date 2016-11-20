using System;
using CodeBox.ObjectModel;
using CodeBox.Affinity;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(IComponent))]
    [ComponentData("command.editor.wordright")]
    public sealed class WordRightCommand : CaretCommand
    {
        protected override Pos GetPosition(Selection sel)
        {
            var pos = WordRight(View, sel);
            sel.SetToRestore(pos);
            return pos;
        }

        internal static Pos WordRight(IEditorView ctx, Selection sel)
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

        public override bool SupportLimitedMode => true;
    }
}
