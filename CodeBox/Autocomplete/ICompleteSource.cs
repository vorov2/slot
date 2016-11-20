using CodeBox.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Autocomplete
{
    public interface ICompleteSource
    {
        void Initialize(IEditorView context);

        IEnumerable<string> GetItems();
    }
}
