using CodeBox.Commands;
using CodeBox.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Folding
{
    public interface IFoldingProvider
    {
        void Fold(IEditorContext ctx, Range range);
    }
}
