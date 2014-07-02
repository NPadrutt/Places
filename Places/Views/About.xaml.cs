using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using Places.Resources;
using Places.Src;
using System.Windows;
using System.Windows.Navigation;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace Places.Views
{
    public partial class About : PhoneApplicationPage
    {
        public About()
        {
            InitializeComponent();

            btnPurchase.Visibility = new Microsoft.Phone.Marketplace.LicenseInformation().IsTrial()
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            lblVersion.Text = Utilities.GetVersion();
        }

        private void lblSupportAddress_Tap(object sender, GestureEventArgs e)
        {
            var mail = new EmailComposeTask { To = AppResources.SupportAddressMail };
            mail.Show();
        }

        private void btnRateApp_Click(object sender, RoutedEventArgs e)
        {
            var marketPlaceReviewTask = new MarketplaceReviewTask();
            marketPlaceReviewTask.Show();
        }

        private void btnPurchase_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var marketPlaceDetailTask = new MarketplaceDetailTask();
            marketPlaceDetailTask.Show();
        }
    }
}