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
            MarketplaceDetailTask _marketPlaceDetailTask = new MarketplaceDetailTask();
            _marketPlaceDetailTask.Show();
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            e.Cancel = true;
        }
    }
}