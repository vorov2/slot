using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slot.Core
{
    public static class StringExtensions
    {
        public static bool ContainsAll(this string str, string[] strings)
        {
            foreach (var s in strings)
            {
                if (str.IndexOf(s.ToString(), StringComparison.OrdinalIgnoreCase) == -1)
                    return false;
            }

            return true;
        }
    }
}
