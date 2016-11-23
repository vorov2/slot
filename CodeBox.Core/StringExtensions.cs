using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Core
{
    public static class StringExtensions
    {
        public static bool IndexOfAll(this string str, char[] chars)
        {
            foreach (var c in chars)
            {
                if (str.IndexOf(c) == -1)
                    return false;
            }

            return true;
        }
    }
}
