using System;
using Slot.Core.ComponentModel;

namespace Slot.Editor.ComponentModel
{
    public interface IDentComponent : IComponent
    {
        int CalculateIndentation(IExecutionContext ctx, int lineIndex);
    }
}
