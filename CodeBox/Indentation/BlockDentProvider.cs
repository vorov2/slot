using System;
using System.ComponentModel.Composition;

namespace CodeBox.Indentation
{
    [Export(typeof(IDentProvider))]
    [ExportMetadata("Key", "indent.block")]
    public sealed class BlockDentProvider : IDentProvider
    {
        public int Calculate(int lineIndex, IEditorContext ctx)
        {
            if (lineIndex > 0)
            {
                var ln = ctx.Buffer.Document.Lines[lineIndex - 1];
                var indent = 0;

                foreach (var c in ln)
                    if (c.Char == ' ')
                        indent++;
                    else if (c.Char == '\t')
                        indent += ctx.TabSize;
                    else
                        break;

                return indent;
            }
            else
                return 0;
        }
    }
}
