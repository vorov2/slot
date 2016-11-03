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
        IEnumerable<string> GetItems(DocumentBuffer buffer);
    }
}
