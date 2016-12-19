using Slot.Core.CommandModel;
using Slot.Core.ComponentModel;
using Slot.Core.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Forms;

namespace Slot.Main.View
{
    [Export(typeof(IArgumentValueProvider))]
    [ComponentData("values.views")]
    public sealed class ViewValueProvider : IArgumentValueProvider
    {
        [Import]
        private IViewManager viewManager = null;

        public IEnumerable<ValueItem> EnumerateArgumentValues(object curvalue)
        {
            var str = curvalue as string;
            var act = viewManager.GetActiveView();
            return viewManager.EnumerateViews()
                .Where(f => str == null || f.Buffer.File.Name.IndexOf(str, StringComparison.OrdinalIgnoreCase) != -1)
                .Select(f => new ValueItem(f.Buffer.File.Name, f == act ? "Current view" : ""));
        }
    }
}
