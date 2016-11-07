using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox
{
    public class CodeBoxException : Exception
    {
        public CodeBoxException(string message) : base(message)
        {

        }

        public CodeBoxException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}
