using System;
using System.ComponentModel.Composition;
using Slot.Core.CommandModel;
using Slot.Core.ComponentModel;

namespace Slot.Main
{
    [Export(typeof(IArgumentValueProvider))]
    [ComponentData("values.infos")]
    public sealed class InfoValueProvider : EnumValueProvider<EnvironmentCommandDispatcher.Info>
    {

    }
}
