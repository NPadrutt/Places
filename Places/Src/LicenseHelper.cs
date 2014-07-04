using BugSense;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Store;

namespace Places.Src
{
    public class LicenseHelper
    {
        public static bool IsLimitExceeded
        {
            get { return App.ViewModel.AllLocations.Count >= 10 && !IsFeaturepackLicensed; }
        }

        private static bool isFeaturepackLicensed;

        public static bool IsFeaturepackLicensed
        {
            get
            {
#if DEBUG
                isFeaturepackLicensed = true;
#endif
                return isFeaturepackLicensed;
            }
        }

        public static async Task CheckLicenceFeaturepack()
        {
            try
            {
                var listing = await CurrentApp.LoadListingInformationAsync();
                var featurepackLicence = listing.ProductListings.FirstOrDefault(p => p.Value.ProductId == "10000");

                if (CurrentApp.LicenseInformation.ProductLicenses != null)
                {
                    isFeaturepackLicensed = CurrentApp.LicenseInformation.ProductLicenses[featurepackLicence.Key].IsActive;
                }
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("0x805A0194"))
                {
                    BugSenseHandler.Instance.LogException(ex);
                }
            }
        }
    }
}