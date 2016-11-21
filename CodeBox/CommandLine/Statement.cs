using CodeBox.Core.ComponentModel;
using System;
using System.Collections.Generic;

namespace CodeBox.CommandLine
{
    public sealed class Statement
    {
        internal Statement()
        {

        }

        public Loc Location { get; internal set; }

        public string Command { get; internal set; }

        public ArgumentType ArgumentType { get; internal set; }
        
        public Loc ArgumentLocation { get; internal set; }

        public object Argument { get; internal set; }
    }
}
