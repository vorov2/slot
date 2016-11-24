using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Core.CommandModel
{
    public class ValueItem
    {
        public ValueItem(string text) : this(text, null)
        {
        }

        public ValueItem(string text, string meta)
        {
            Value = text;
            Meta = meta;
        }

        protected ValueItem()
        {

        }

        public virtual string Value { get; }

        public virtual string Meta { get; }

        public override string ToString() => (Value ?? "").ToString();

        //public virtual void Draw(Action<string,Rectangle> drawMain)
    }
}
