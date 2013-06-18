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

            this.DataContext = App.ViewModel.SelectedLocation;

            (ApplicationBar.Buttons[0] as ApplicationBarIconButton).Text = AppResources.EditLabel;
            (ApplicationBar.Buttons[1] as ApplicationBarIconButton).Text = AppResources.DeleteLabel;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (this.NavigationContext.QueryString != null && this.NavigationContext.QueryString.Count > 0)
            {
                if (this.NavigationContext.QueryString.ContainsKey("RemoveBackstack") && Convert.ToBoolean(this.NavigationContext.QueryString["RemoveBackstack"]))
                {
                    NavigationService.RemoveBackEntry();
                }
            }
        }

        private void btnDelete_Click(object sender, System.EventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(AppResources.DeleteMessage, AppResources.DeleteMessageTitle, MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.OK)
            {
                App.ViewModel.DeleteLocation(App.ViewModel.SelectedLocation);
            }

            NavigationService.GoBack();
        }

        private void btnEdit_Click(object sender, System.EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/AddLocation.xaml", UriKind.Relative));
        }

        private void StackPanel_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/MapView.xaml", UriKind.Relative));
        }
    }
}