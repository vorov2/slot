using CodeBox.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Styling
{
    public interface IStylingProvider
    {
        void Style(IEditorContext context, Range range);
    }
}
