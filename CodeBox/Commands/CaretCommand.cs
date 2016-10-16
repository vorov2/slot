using System;
using CodeBox.ObjectModel;

namespace CodeBox.Commands
{
    internal abstract class CaretCommand : Command
    {
        public override void Execute(EditorContext context, Selection sel)
        {
            var pos = GetPosition(context, sel.Caret);
            sel.Clear(pos);
            context.ValidateCaret(sel);
        }

        protected abstract Pos GetPosition(EditorContext context, Pos caret);
    }
}
