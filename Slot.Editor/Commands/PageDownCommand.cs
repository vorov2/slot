using System;
using Slot.Editor.ObjectModel;
using System.ComponentModel.Composition;
using Slot.Core.ComponentModel;

namespace Slot.Editor.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.pagedown")]
    public sealed class PageDownCommand : CaretCommand
    {
        protected override Pos GetPosition(Selection sel) => PageDown(Ed);

        internal static Pos PageDown(EditorControl ctx)
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
