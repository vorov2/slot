using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.Core.ComponentModel;
using CodeBox.Core.Keyboard;

namespace CodeBox.Core.CommandModel
{
    public sealed class CommandMetadata
    {
        public Identifier Key { get; internal set; }

        public Identifier Mode { get; internal set; }

        public string Shortcut { get; internal set; }

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
