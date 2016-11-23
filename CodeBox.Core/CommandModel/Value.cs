using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Core.CommandModel
{
    public class Value
    {
        public object Data { get; set; }

        public override string ToString() => (Data ?? "").ToString();
    }
}
