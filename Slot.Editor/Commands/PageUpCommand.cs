using System;
using Slot.Editor.ObjectModel;
using Slot.Editor.Folding;
using Slot.ComponentModel;
using System.ComponentModel.Composition;
using Slot.Core.ComponentModel;

namespace Slot.Editor.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.pageup")]
    public sealed class PageUpCommand : CaretCommand
    {
        protected override Pos GetPosition(Selection sel) => PageUp(Ed);

        internal static Pos PageUp(EditorControl ctx)
        {
            var lines = ctx.Buffer.Document.Lines;
            var caret = ctx.Buffer.Selections.Main.Caret;
            var line = lines[caret.Line];
            var stripes = 0;
            var lastLine = default(Line);
            var lastLineIndex = 0;

            for (var i = caret.Line; i > -1; i--)
            {
                if (!ctx.Folding.IsLineVisible(i))
                    continue;

                var ln = lines[i];
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
