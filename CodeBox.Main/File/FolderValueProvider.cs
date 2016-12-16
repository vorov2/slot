using System;
using Slot.Core.CommandModel;
using Slot.Core.ComponentModel;
using System.ComponentModel.Composition;

namespace Slot.Main.File
{
    [Export(typeof(IArgumentValueProvider))]
    [ComponentData("values.folders")]
    public sealed class FolderValueProvider : SystemPathValueProvider
    {
        protected override bool IncludeFiles => false;
    }
}
