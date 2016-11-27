using CodeBox.ComponentModel;
using System;
using System.ComponentModel.Composition;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Indentation
{
    [Export(typeof(IComponent))]
    [ComponentData("indent.block")]
    public sealed class BlockDentProvider : IDentComponent
    {
        public int CalculateIndentation(IExecutionContext context, int lineIndex)
        {
            var ctx = (Editor)context;

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
