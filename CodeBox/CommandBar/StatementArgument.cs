using CodeBox.Core.CommandModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.CommandBar
{
    public sealed class StatementArgument : CommandLineItem
    {
        public StatementArgument(object value) : this()
        {
            Value = value;
        }

        internal StatementArgument()
        {
            Location = new Loc(-1, -1);
        }

        public object Value { get; internal set; }
    }
}
