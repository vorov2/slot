using System;

namespace CodeBox.CommandBar
{
    public abstract class CommandLineItem
    {
        protected CommandLineItem()
        {

        }

        public Loc Location { get; internal set; }
    }
}
