using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Places.Src
{
    public class Products
    {
        public Products(string token, string name)
        {
            Token = token;
            Name = name;
        }

        public string Token { get; private set; }

        public string Name { get; private set; }

        #region Getter for Products

        public static Products RemoveAds
        {
            get
            {
                return new Products("RemoveAds", "Remove Ads");
            }
        }

        #endregion
    }
}
