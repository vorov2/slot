using CodeBox.Core.ComponentModel;
using CodeBox.Core.Output;
using CodeBox.ObjectModel;
using System;
using System.ComponentModel.Composition;

namespace CodeBox.Test
{
    [Export(typeof(ILogComponent))]
    [ComponentData("log.application")]
    public sealed class Logs : LogBuffer
    {
    }
}
