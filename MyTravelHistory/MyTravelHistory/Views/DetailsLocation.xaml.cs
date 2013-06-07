using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using MyTravelHistory.Resources;

namespace MyTravelHistory.Views
{
    public partial class DetailsLocation : PhoneApplicationPage
    {
        public DetailsLocation()
        {
            InitializeComponent();
        }

        private void btnDelete_Click(object sender, System.EventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(AppResources.DeleteMessage, AppResources.DeleteMessageTitle, MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.OK)
            {
                App.ViewModel.DeleteLocation(App.ViewModel.SelectedLocations);
            }

            NavigationService.GoBack();
        }

        private void btnEdit_Click(object sender, System.EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/EditLocation.xaml", UriKind.Relative));
        }
    }
}