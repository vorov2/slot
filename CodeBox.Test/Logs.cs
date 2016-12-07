using CodeBox.Core.ComponentModel;
using CodeBox.Core.Output;
using CodeBox.ObjectModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Test
{
    [Export(typeof(ILogComponent))]
    [ComponentData("log.application")]
    public sealed class Logs : LogBuffer
    {
    }
}
