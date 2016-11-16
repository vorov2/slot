using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.ComponentModel
{
    public interface IDentComponent : IComponent
    {
        int CalculateIndentation(IEditorContext ctx, int lineIndex);
    }
}
