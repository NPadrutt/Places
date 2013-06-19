using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTravelHistory.Src
{
    public class Utilities
    {
        public static string GetVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().FullName.Split('=')[1].Split(',')[0];
        }
    }
}
