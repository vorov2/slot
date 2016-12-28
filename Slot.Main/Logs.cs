using Slot.Core.ComponentModel;
using Slot.Core.Output;
using Slot.Editor.ObjectModel;
using System;
using System.ComponentModel.Composition;

namespace Slot.Main
{
    [Export(typeof(ILogComponent))]
    [ComponentData("log.application")]
    public sealed class Logs : LogBuffer
    {

    }
}
