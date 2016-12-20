using System;
using Slot.Core.CommandModel;
using Slot.Core.ComponentModel;
using System.ComponentModel.Composition;
using Slot.Main.File;

namespace Slot.Main.Workspace
{
    [Export(typeof(IArgumentValueProvider))]
    [ComponentData("values.folders")]
    public sealed class FolderValueProvider : SystemPathValueProvider
    {
        protected override bool IncludeFiles => false;
    }
}
