using System;

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

        public static int LongestCommonSubstring(this string self, string other)
        {
            var output = "";
            var lenx = self.Length;

            for (var j = 0; j < lenx; j++)
            {
                for (var k = lenx - j; k > -1; k--)
                {
                    var common = self.Substring(j, k);
                    if (other.IndexOf(common, StringComparison.OrdinalIgnoreCase) > -1 
                        && common.Length > output.Length)
                        output = common;
                }
            }

            return output.Length;
        }

        public static int LongestCommonSubsequence(this string self, string other, int index1 = 0, int index2 = 0)
        {
            var max = 0;

            if (index1 == self.Length)
                return 0;

            if (index2 == other.Length)
                return 0;

            for (var i = index1; i < self.Length; i++)
            {
                var exist = other.IndexOf(self[i].ToString(), index2, StringComparison.OrdinalIgnoreCase);

                if (exist != -1)
                {
                    var temp = 1 + LongestCommonSubsequence(self, other, i + 1, exist + 1);

                    if (max < temp)
                        max = temp;
                }
            }

            return max;
        }

        public static int LevenshteinDistance(this string self, string other)
        {
            if (self == null || other == null)
                return 0;

            var n = self.Length;
            var m = other.Length;
            int[,] d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0)
                return m;

            if (m == 0)
                return n;

            // Step 2
            for (var i = 0; i <= n; d[i, 0] = i++) { }
            for (var j = 0; j <= m; d[0, j] = j++) { }

            // Step 3
            for (var i = 1; i <= n; i++)
            {
                //Step 4
                for (var j = 1; j <= m; j++)
                {
                    // Step 5
                    var cost = (other[j - 1] == self[i - 1]) ? 0 : 1;

                    // Step 6
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }

            // Step 7
            return d[n, m];
        }
    }
}
