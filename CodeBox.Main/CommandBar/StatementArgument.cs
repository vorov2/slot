using System;

namespace CodeBox.Main.CommandBar
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
