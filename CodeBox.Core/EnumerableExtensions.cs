using System;
using System.Collections.Generic;

namespace CodeBox.Core
{
    public static class EnumerableExtensions
    {
        public static bool JustOne<T>(this IEnumerable<T> seq)
        {
            var c = 0;
            foreach (var _ in seq)
                if (c++ > 0)
                    return false;

            return c == 1;
        }
    }
}
