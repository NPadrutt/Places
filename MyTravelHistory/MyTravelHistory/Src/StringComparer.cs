using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTravelHistory.Src
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
