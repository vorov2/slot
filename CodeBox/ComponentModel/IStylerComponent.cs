using Slot.Editor.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slot.Core.ComponentModel;

namespace Slot.ComponentModel
{
    public interface IStylerComponent : IComponent
    {
        void Style(IExecutionContext context, Range range);
    }
}
