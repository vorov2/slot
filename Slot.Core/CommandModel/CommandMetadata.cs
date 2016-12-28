using System;
using System.Collections.Generic;
using System.Text;

namespace Slot.Core.CommandModel
{
    public sealed class CommandMetadata : IEquatable<CommandMetadata>
    {
        public Identifier Key { get; internal set; }

        public Identifier Mode { get; internal set; }

        public string Shortcut { get; set; }

        public string Alias { get; internal set; }

        public string Title { get; internal set; }

        public bool HasArguments => _arguments != null && _arguments.Count > 0;

        private List<ArgumentMetadata> _arguments;
        public List<ArgumentMetadata> Arguments
        {
            get
            {
                if (_arguments == null)
                    _arguments = new List<ArgumentMetadata>();

                return _arguments;
            }
        }

        public override int GetHashCode() => Key.GetHashCode();

        public bool Equals(CommandMetadata other) => other != null && Key.Equals(other.Key);

        public override bool Equals(object obj) => Equals(obj as CommandMetadata);

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(Key.Name);

            if (HasArguments)
            {
                for (var i = 0; i < Arguments.Count; i++)
                {
                    var ar = Arguments[i];

                    if (i == 0)
                        sb.Append(' ');
                    else
                        sb.Append(" | ");

                    sb.Append(ar);

                    if (i == Arguments.Count - 1)
                        sb.Append(' ');
                }
            }

            if (Title != null)
            {
                var str = Title;
                var idx = str.IndexOf(':');
                if (idx != -1)
                    str = str.Substring(idx + 1).Trim();
                sb.Append(" #" + str);
            }

            return sb.ToString();
        }
    }
}
