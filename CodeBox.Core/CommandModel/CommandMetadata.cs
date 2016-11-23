using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Core.CommandModel
{
    public sealed class CommandMetadata
    {
        public Identifier Key { get; internal set; }

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
    }

    public sealed class ArgumentMetadata
    {
        public string Name { get; internal set; }

        public Identifier ValueProvider { get; internal set; }

        public ArgumentType Type { get; internal set; }
    }
}
