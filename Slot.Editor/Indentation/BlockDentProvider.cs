using System;
using System.ComponentModel.Composition;
using Slot.Core.ComponentModel;
using Slot.Core.ViewModel;
using Slot.Editor.ComponentModel;

namespace Slot.Editor.Indentation
{
    [Export(typeof(IDentComponent))]
    [ComponentData(Name)]
    public sealed class BlockDentProvider : IDentComponent
    {
        public const string Name = "ident.block";

        public int CalculateIndentation(IView view, int lineIndex)
        {
            var editor = (EditorControl)view;

            if (lineIndex > 0)
            {
                var ln = editor.Buffer.Document.Lines[lineIndex - 1];
                var indent = 0;

                foreach (var c in ln)
                    if (c.Char == ' ')
                        indent++;
                    else if (c.Char == '\t')
                        indent += editor.IndentSize;
                    else
                        break;

                return indent;
            }
            else
                return 0;
        }
    }
}
