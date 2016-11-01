using CodeBox.Commands;
using CodeBox.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Indentation
{
    public interface IDentProvider
    {
        int Calculate(int lineIndex, IEditorContext ctx);
    }
}
