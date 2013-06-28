using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using MyTravelHistory.Resources;
using MyTravelHistory.Src;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace MyTravelHistory.Views
{
    public partial class About : PhoneApplicationPage
    {
        public About()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            lblVersion.Text = Utilities.GetVersion();
        }

        private void lblSupportAddress_Tap(object sender, GestureEventArgs e)
        {
            var mail = new EmailComposeTask();
            mail.To = AppResources.SupportAddressMail;
            mail.Show();
        }

        private void btnRateApp_Click(object sender, RoutedEventArgs e)
        {
            var marketPlaceReviewTask = new MarketplaceReviewTask();
            marketPlaceReviewTask.Show();
        }
    }
}