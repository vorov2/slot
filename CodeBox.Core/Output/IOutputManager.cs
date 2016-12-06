using CodeBox.Core;
using CodeBox.Core.ComponentModel;
using CodeBox.Core.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Core.Output
{
    public interface IOutputManager : IComponent
    {
        ILog GetOutput(Identifier key);
    }
}
