using System;

namespace CodeBox.ObjectModel
{
    internal sealed class CommandInfo
    {
        public int Id { get; set; }

        public ICommand Command { get; set; }

        public ActionExponent Exponent { get; set; }
    }
}
