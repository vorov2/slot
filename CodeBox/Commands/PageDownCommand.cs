using System;
using CodeBox.ObjectModel;

namespace CodeBox.Commands
{
    [CommandBehavior(ActionExponent.Scroll | ActionExponent.ClearSelections)]
    internal sealed class PageDownCommand : CaretCommand
    {
        protected override Pos GetPosition(EditorContext context, Pos caret)
        {
            return PageDown(context);
        }

        internal static Pos PageDown(EditorContext context)
        {
            var lines = context.Document.Lines;
            var caret = context.Document.Selections.Main.Caret;
            var line = lines[caret.Line];
            var stripes = 0;
            var lastLine = default(Line);

            for (var i = line.Index; i < lines.Count; i++)
            {
                lastLine = lines[i];
                stripes += lastLine.Stripes;

                if (stripes >= context.StripesPerScreen)
                    break;
            }

            return new Pos(lastLine.Index, caret.Col > lastLine.Length ? lastLine.Length : caret.Col);
        }
    }
}
