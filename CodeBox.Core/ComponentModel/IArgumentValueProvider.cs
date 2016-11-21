using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Core.ComponentModel
{
    public interface IArgumentValueProvider
    {
        IEnumerable<ArgumentValue> EnumerateArgumentValues(object curvalue);
    }

    public class ArgumentValue
    {
        public object Value { get; set; }

        public override string ToString() => (Value ?? "").ToString();
    }
}
