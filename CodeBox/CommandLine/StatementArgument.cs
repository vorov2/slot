using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.CommandLine
{
    public sealed class StatementArgument : CommandLineItem
    {
        internal StatementArgument()
        {

        }

        public object Value { get; internal set; }
    }
}
