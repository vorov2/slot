﻿using System;
using Slot.Editor.ObjectModel;
using System.ComponentModel.Composition;
using static Slot.Editor.Commands.ActionResults;
using Slot.Core.ComponentModel;

namespace Slot.Editor.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData(Name)]
    public class DeleteAllRightCommand : DeleteRangeCommand
    {
        public const string Name = "editor.deleteAllRight";

        internal override ActionResults Execute(Selection sel, params object[] args)
        {
            if (!sel.IsEmpty)
                return base.Execute(sel, args);
            else
            {
                var ln = Document.Lines[sel.Caret.Line];

                if (sel.Caret.Col == ln.Length)
                    return Pure;

                sel.Start = sel.Caret;
                sel.End = new Pos(sel.Caret.Line, ln.Length);
                return base.Execute(sel, args);
            }
        }

        internal override EditorCommand Clone()
        {
            return new DeleteAllRightCommand();
        }

        internal override bool ModifyContent => true;
    }
}
