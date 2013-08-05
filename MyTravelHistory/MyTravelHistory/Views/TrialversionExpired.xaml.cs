using System.Windows;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;

namespace MyTravelHistory.Views
{
    public partial class TrialversionExpired : PhoneApplicationPage
    {
        public TrialversionExpired()
        {
            InitializeComponent();
        }

        private void btnPurchase_Click(object sender, RoutedEventArgs e)
        {
            var marketPlaceDetailTask = new MarketplaceDetailTask();
            marketPlaceDetailTask.Show();
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            e.Cancel = true;
        }
    }
}