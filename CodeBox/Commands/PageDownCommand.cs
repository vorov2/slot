using System;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(Scroll | ClearSelections)]
    public sealed class PageDownCommand : CaretCommand
    {
        protected override Pos GetPosition(Selection sel) => PageDown(Context);

        internal static Pos PageDown(IEditorContext ctx)
        {
            var lines = ctx.Buffer.Document.Lines;
            var caret = ctx.Buffer.Selections.Main.Caret;
            var line = lines[caret.Line];
            var stripes = 0;
            var lastLine = default(Line);
            var lastLineIndex = 0;

            for (var i = caret.Line; i < lines.Count; i++)
            {
                lastLine = lines[i];
                lastLineIndex = i;
                stripes += lastLine.Stripes;

                if (stripes >= ctx.Info.StripesPerScreen)
                    break;
            }

            return new Pos(lastLineIndex, caret.Col > lastLine.Length ? lastLine.Length : caret.Col);
        }
    }
}
