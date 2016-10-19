using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Drawing
{
    internal sealed class Renderer
    {
        private readonly Editor editor;

        internal Renderer(Editor editor)
        {
            this.editor = editor;
        }
    }
}
