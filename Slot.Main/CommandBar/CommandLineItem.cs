using System;

namespace Slot.Main.CommandBar
{
    public abstract class CommandLineItem
    {
        protected CommandLineItem()
        {

        }

        public Loc Location { get; internal set; }
    }
}
