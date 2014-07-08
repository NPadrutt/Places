using BugSense;
using Places.Models;
using Places.Resources;
using Places.Src;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Windows.ApplicationModel.Store;

namespace Places.UserControls
{
    public partial class FeatureOverviewUserControl
    {
        public FeatureOverviewUserControl()
        {
            InitializeComponent();
            RenderStoreItems();
        }

        public ObservableCollection<ProductItem> picItems = new ObservableCollection<ProductItem>();

        private async void RenderStoreItems()
        {
            picItems.Clear();

            try
            {
                var li = await CurrentApp.LoadListingInformationAsync();

                foreach (string key in li.ProductListings.Keys)
                {
                    ProductListing pListing = li.ProductListings[key];
                    var status = CurrentApp.LicenseInformation.ProductLicenses[key].IsActive ? AppResources.PurchasedLabel : pListing.FormattedPrice;

                    picItems.Add(
                        new ProductItem
                        {
                            imgLink = key.Equals("10001") ? "/Images/{0}/UnlockFeatures.png" : "/Images/add.png.png",
                            Name = pListing.Name,
                            Status = status,
                            key = key,
                            Description = pListing.Description,
                            BuyNowButtonVisible = CurrentApp.LicenseInformation.ProductLicenses[key].IsActive ? Visibility.Collapsed : Visibility.Visible
                        }
                    );
                }

                Plugin.ItemsSource = picItems;
            }
            catch (Exception ex)
            {
                BugSenseHandler.Instance.LogException(ex);
            }
        }

        private async void ButtonBuyNow_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                var btn = sender as Button;

                var key = btn.Tag.ToString();

                if (!CurrentApp.LicenseInformation.ProductLicenses[key].IsActive)
                {
                    var li = await CurrentApp.LoadListingInformationAsync();
                    string pID = li.ProductListings[key].ProductId;

                    await CurrentApp.RequestProductPurchaseAsync(pID, true);
                    await LicenseHelper.CheckLicenceFeaturepack();
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("0x80004005"))
                {
                    MessageBox.Show(AppResources.PurchasedFailedText, AppResources.PurchasedFailedTitle,
                        MessageBoxButton.OK);
                }
                else
                {
                    BugSenseHandler.Instance.LogException(ex);
                }
            }
        }
    }
}