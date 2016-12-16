using Slot.Core.ComponentModel;
using Slot.Editor.ObjectModel;
using System;

namespace Slot.Editor.ComponentModel
{
    public interface IFoldingComponent : IComponent
    {
        void Fold(IExecutionContext context, Range range);
    }
}
