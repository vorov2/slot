using System;
using CodeBox.ObjectModel;
using CodeBox.Folding;

namespace CodeBox.Commands
{
    public sealed class PageUpCommand : CaretCommand
    {
        protected override Pos GetPosition(Selection sel) => PageUp(Context);

        internal static Pos PageUp(IEditorContext ctx)
        {
            var lines = ctx.Buffer.Document.Lines;
            var caret = ctx.Buffer.Selections.Main.Caret;
            var line = lines[caret.Line];
            var stripes = 0;
            var lastLine = default(Line);
            var lastLineIndex = 0;

            for (var i = caret.Line; i > -1; i--)
            {
                var ln = lines[i];

                if (ln.Folding.Has(FoldingStates.Invisible))
                    continue;

                lastLine = ln;
                lastLineIndex = i;
                stripes += lastLine.Stripes;

                if (stripes >= ctx.Info.StripesPerScreen)
                    break;
            }

            return new Pos(lastLineIndex, caret.Col > lastLine.Length ? lastLine.Length : caret.Col);
        }
    }
}
