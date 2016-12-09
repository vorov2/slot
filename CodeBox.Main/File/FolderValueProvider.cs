using System;
using CodeBox.Core.CommandModel;
using CodeBox.Core.ComponentModel;
using System.ComponentModel.Composition;

namespace CodeBox.Main.File
{
    [Export(typeof(IArgumentValueProvider))]
    [ComponentData("values.folders")]
    public sealed class FolderValueProvider : SystemPathValueProvider
    {
        protected override bool IncludeFiles => false;
    }
}
