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

        public IEnumerable<ValueItem> EnumerateArgumentValues()
        {
            var act = viewManager.ActiveView;
            return viewManager.EnumerateViews()
                .Select(f => new ValueItem(f.Buffer.File.Name, f == act ? "Current view" : ""));
        }
    }
}
