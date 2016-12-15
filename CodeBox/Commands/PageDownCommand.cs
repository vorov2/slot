using System;
using CodeBox.ObjectModel;
using CodeBox.Folding;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.pagedown")]
    public sealed class PageDownCommand : CaretCommand
    {
        protected override Pos GetPosition(Selection sel) => PageDown(View);

        internal static Pos PageDown(Editor ctx)
        {
            var lines = ctx.Buffer.Document.Lines;
            var caret = ctx.Buffer.Selections.Main.Caret;
            var line = lines[caret.Line];
            var stripes = 0;
            var lastLine = default(Line);
            var lastLineIndex = 0;

            for (var i = caret.Line; i < lines.Count; i++)
            {
                if (!ctx.Folding.IsLineVisible(i))
                    continue;

                var ln = lines[i];
                lastLine = ln;
                lastLineIndex = i;
                stripes += ln.Stripes;

                if (stripes >= ctx.Info.StripesPerScreen)
                    break;
            }

            return new Pos(lastLineIndex, caret.Col > lastLine.Length ? lastLine.Length : caret.Col);
        }
    }
}
