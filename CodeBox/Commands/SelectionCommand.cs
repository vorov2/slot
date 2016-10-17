using System;
using CodeBox.ObjectModel;
using static CodeBox.ObjectModel.ActionExponent;

namespace CodeBox.Commands
{
    internal abstract class SelectionCommand : Command
    {
        public override void Execute(EditorContext context, Selection sel)
        {
            var doc = context.Document;
            var pos = Select(context, sel.Caret);
            sel.End = pos;
        }

        protected abstract Pos Select(EditorContext context, Pos pos);
    }
}
