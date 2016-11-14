using System;
using System.Collections.Generic;

namespace Json
{
    public sealed class DictionaryComparer : IEqualityComparer<string>
    {
        public static readonly DictionaryComparer Instance = new DictionaryComparer();

        private DictionaryComparer()
        {

        }

        public bool Equals(string x, string y)
        {
            return string.Equals(x, y, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(string x)
        {
            return x.GetHashCode();
        }
    }
}