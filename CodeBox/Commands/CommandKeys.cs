using System;
using System.Windows.Forms;

namespace CodeBox.Commands
{
    internal struct CommandKeys : IEquatable<CommandKeys>
    {
        public CommandKeys(MouseEvents mouse, Keys keys)
        {
            Mouse = mouse;
            Keys = keys;
        }

        public readonly Keys Keys;
        public readonly MouseEvents Mouse;

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Keys.GetHashCode();
                hash = hash * 23 + Mouse.GetHashCode();
                return hash;
            }

        }

        public override bool Equals(object obj) =>
            obj is CommandKeys ? Equals((CommandKeys)obj) : false;

        public bool Equals(CommandKeys other) =>
            Keys == other.Keys && Mouse == other.Mouse;

        public override string ToString() => $"{Mouse}, {Keys}";
    }
}
