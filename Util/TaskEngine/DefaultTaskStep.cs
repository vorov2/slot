using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;

namespace TaskEngine
{
    public sealed class DefaultTaskStep : TaskStep
    {
        public DefaultTaskStep(Task parent) : base(parent)
        {

        }

        public override ExecMonitor Start(IOutput output)
        {
            var psi = CreateProcessStartInfo();
            return RunProcess(psi, output, WriteOutput);
        }
    }
}