using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;
using CodeBox.Commands;

namespace CodeBox.Indentation
{
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
                        indent += ctx.Settings.TabSize;
                    else
                        break;

                return indent;
            }
            else
                return 0;
        }
    }
}
