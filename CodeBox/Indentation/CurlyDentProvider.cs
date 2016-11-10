using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;
using CodeBox.Commands;

namespace CodeBox.Indentation
{
    public sealed class CurlyDentProvider : IDentProvider
    {
        public int Calculate(int lineIndex, IEditorContext ctx)
        {
            if (lineIndex > 0)
            {
                var ln = ctx.Buffer.Document.Lines[lineIndex - 1];
                var idx = ln.Length - 1;
                var indent = 0;
                var curly = false;

                while (idx > -1)
                {
                    var ch = ln[idx--];

                    if (ch.Char == '{')
                    {
                        curly = true;
                        break;
                    }
                    else if (ch.Char != ' ')
                        break;
                }

                foreach (var c in ln)
                    if (c.Char == ' ')
                        indent++;
                    else if (c.Char == '\t')
                        indent += ctx.TabSize;
                    else
                        break;
                
                if (curly)
                    indent += ctx.TabSize;

                return indent;
            }
            else
                return 0;
        }
    }
}
