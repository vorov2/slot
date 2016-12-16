using Slot.Editor.ComponentModel;
using System;
using System.ComponentModel.Composition;
using Slot.Core.ComponentModel;

namespace Slot.Editor.Indentation
{
    [Export(typeof(IDentComponent))]
    [ComponentData(Name)]
    public sealed class BlockDentProvider : IDentComponent
    {
        public const string Name = "ident.block";

        public int CalculateIndentation(IExecutionContext context, int lineIndex)
        {
            var ctx = (EditorControl)context;

            if (lineIndex > 0)
            {
                var ln = ctx.Buffer.Document.Lines[lineIndex - 1];
                var indent = 0;

                foreach (var c in ln)
                    if (c.Char == ' ')
                        indent++;
                    else if (c.Char == '\t')
                        indent += ctx.IndentSize;
                    else
                        break;

                return indent;
            }
            else
                return 0;
        }
    }
}
