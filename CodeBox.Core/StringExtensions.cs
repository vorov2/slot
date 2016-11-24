using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Core
{
    public static class StringExtensions
    {
        public static bool ContainsAll(this string str, char[] chars)
        {
            foreach (var c in chars)
            {
                if (str.IndexOf(c.ToString(), StringComparison.OrdinalIgnoreCase) == -1)
                    return false;
            }

            return true;
        }
    }
}
