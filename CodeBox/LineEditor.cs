using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox
{
    internal sealed class LineEditor : Editor
    {
        internal LineEditor(Editor editor) : base(editor.Settings)
        {
            LimitedMode = true;
            Height = editor.Info.LineHeight;
        }
    }
}
