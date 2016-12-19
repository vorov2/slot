using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Json;

namespace Slot.Core
{
    public sealed class Identifier : IEquatable<Identifier>
    {
        private readonly int hash;

        public Identifier(string ident)
        {
            if (ident == null)
                throw new ArgumentNullException(nameof(ident));

            hash = ident.ToUpper().GetHashCode();
            ParseIdent(ident);
        }

        private void ParseIdent(string ident)
        {
            for (var i = ident.Length - 1; i > 0; i--)
            {
                if (ident[i] == '.')
                {
                    Namespace = new Identifier(ident.Substring(0, i));
                    Name = ident.Substring(i + 1);
                    return;
                }
            }

            Name = ident;
        }

        public static explicit operator Identifier(string str) => str != null ? new Identifier(str) : null;

        public static explicit operator string(Identifier ident) => ident.ToString();

        public bool Equals(Identifier other) => Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase) 
            && (Namespace == null && other.Namespace == null || Namespace.Equals(other.Namespace));

        public override bool Equals(object other) => other is Identifier && Equals(this, (Identifier)other);

        public override int GetHashCode() => hash;

        public override string ToString() => Namespace != null ? Namespace + "." + Name : Name;

        public Identifier Namespace { get; private set; }

        public string Name { get; private set; }
    }
}
