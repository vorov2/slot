using System;
using System.IO;
using Slot.Core.ComponentModel;

namespace Slot.Core.State
{
    public interface IStateManager : IComponent
    {
        Stream WriteState(Guid stateId);

        Stream ReadState(Guid stateId);
    }
}
