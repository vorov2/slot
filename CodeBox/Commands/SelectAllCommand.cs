﻿using System;
using CodeBox.ObjectModel;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using static CodeBox.Commands.ActionResults;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(ICommandComponent))]
    [CommandComponentData("editor.selectall", "esa")]
    public sealed class SelectAllCommand : EditorCommand
    {
        protected override ActionResults Execute(Selection sel)
        {
            var idx = Document.Lines.Count - 1;
            var ln = Document.Lines[idx];
            sel.Start = default(Pos);
            sel.End = new Pos(idx, ln.Length);
            return Clean | Scroll;
        }

        public override bool SingleRun => true;

        public override bool SupportLimitedMode => true;
    }
}
