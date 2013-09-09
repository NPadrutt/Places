using System;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows;
using Places.Src;
using Windows.ApplicationModel.Store;

namespace Places.UserControls
{
    public partial class CustomAdControl
    {
        public CustomAdControl()
        {
            InitializeComponent();

            CheckLicense();
        }

        private async void CheckLicense()
        {
            try
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains(Product.RemoveAds().Id) &&
                    (bool) IsolatedStorageSettings.ApplicationSettings[Product.RemoveAds().Id])
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        Visibility = Visibility.Collapsed;
                    });
                }
                else
                {
                    var listing = await CurrentApp.LoadListingInformationAsync();
                    var removedAds =
                        listing.ProductListings.FirstOrDefault(p => p.Value.ProductId == Product.RemoveAds().Id);

                    IsolatedStorageSettings.ApplicationSettings.Add(removedAds.Key,
                                                                    CurrentApp.LicenseInformation.ProductLicenses[
                                                                        removedAds.Key].IsActive);
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("0x805A0194"))
                {
                    FlurryWP8SDK.Api.LogError("Task couldn't coulnd't finish with a result.", ex);
                }
            }
        }

        private void MSAdControl_AdRefreshed(object sender, System.EventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                AdDuplexAd.Visibility = Visibility.Collapsed;
                MSAdControl.Visibility = Visibility.Visible;
            });
        }

        private void MSAdControl_ErrorOccurred(object sender, Microsoft.Advertising.AdErrorEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                MSAdControl.Visibility = Visibility.Collapsed;
                AdDuplexAd.Visibility = Visibility.Visible;
            });
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            CheckLicense();
        }
    }
}
