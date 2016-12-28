using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Slot.Core;
using Slot.Core.CommandModel;
using Slot.Core.ComponentModel;

namespace Slot.Editor.BufferCommands
{
    [Export(typeof(IArgumentValueProvider))]
    [ComponentData("values.eol")]
    public sealed class EolValueProvider : EnumValueProvider<Eol>
    {

    }
}
