using Slot.Core.ComponentModel;
using Slot.Editor.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slot.Editor.Autocomplete
{
    public interface ICompleteSource
    {
        void Initialize(IExecutionContext context);

        IEnumerable<string> GetItems();
    }
}
