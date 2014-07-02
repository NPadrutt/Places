using System;
using System.Collections.Generic;

namespace Places.Src
{
    public class StringComparer : IEqualityComparer<String>
    {
        public bool Equals(string x, string y)
        {
            if (string.Compare(x, y, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public int GetHashCode(string obj)
        {
            return obj.GetHashCode();
        }
    }
}