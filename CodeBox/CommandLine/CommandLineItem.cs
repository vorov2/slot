using System;

namespace CodeBox.CommandLine
{
    public abstract class CommandLineItem
    {
        protected CommandLineItem()
        {

        }

        public Loc Location { get; internal set; }
    }
}
