using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Core.CommandModel
{
    public class Value
    {
        public Value(object data)
        {
            Data = data;
        }

        protected Value()
        {

        }

        public virtual object Data { get; }

        public virtual string Meta { get; }

        public override string ToString() => (Data ?? "").ToString();

        //public virtual void Draw(Action<string,Rectangle> drawMain)
    }
}
