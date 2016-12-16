using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Slot.Core.ComponentModel;
using Slot.Editor.ObjectModel;
using static Slot.Editor.Commands.ActionResults;

namespace Slot.Editor.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData(Name)]
    public sealed class AddSelectionCaretsCommand : EditorCommand
    {
        public const string Name = "editor.addCaretsToSelection";

        internal override ActionResults Execute(Selection sel, params object[] args)
        {
            var norm = Buffer.Selections.Main.Normalize();
            var sels = new List<Selection>();

            for (var i = norm.Start.Line; i < norm.End.Line + 1; i++)
                sels.Add(new Selection(new Pos(i, Document.Lines[i].Length)));

            if (sels.Count > 0)
            {
                Buffer.Selections.Clear();

                foreach (var s in sels)
                    Buffer.Selections.AddFast(s);
            }

            return Clean;
        }

        internal override bool SingleRun => true;
    }
}
