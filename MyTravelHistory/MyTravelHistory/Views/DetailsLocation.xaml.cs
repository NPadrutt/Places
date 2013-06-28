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
using MyTravelHistory.Src;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace MyTravelHistory.Views
{
    public partial class DetailsLocation : PhoneApplicationPage
    {
        public DetailsLocation()
        {
            InitializeComponent();

            DataContext = App.ViewModel.SelectedLocation;

            (ApplicationBar.Buttons[0] as ApplicationBarIconButton).Text = AppResources.EditLabel;
            
			(ApplicationBar.MenuItems[0] as ApplicationBarMenuItem).Text = AppResources.DeleteLabel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (NavigationContext.QueryString != null && NavigationContext.QueryString.Count > 0)
            {
                if (NavigationContext.QueryString.ContainsKey("RemoveBackstack") && Convert.ToBoolean(NavigationContext.QueryString["RemoveBackstack"]))
                {
                    NavigationService.RemoveBackEntry();
                }
            }
        }
		
        private void btnEdit_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/AddLocation.xaml", UriKind.Relative));
        }

        private void StackPanel_Tap(object sender, GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/MapView.xaml", UriKind.Relative));
        }

        private void locationImage_Tap(object sender, GestureEventArgs e)
        {
            ImageViewer.IsOpen = true;
        }

        private void locationImage_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.ViewModel.SelectedLocation.LocationImage != null)
            {
                LocationImage.Source = Utilities.ConvertToImage(App.ViewModel.SelectedLocation.LocationImage);
            }
        }

        private void LocationImageLarge_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.ViewModel.SelectedLocation.LocationImage != null)
            {
                locationImageLarge.Source = Utilities.ConvertToImage(App.ViewModel.SelectedLocation.LocationImage);
            }
        }

        private void mDelete_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(AppResources.DeleteMessage, AppResources.DeleteMessageTitle, MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.OK)
            {
                App.ViewModel.DeleteLocation(App.ViewModel.SelectedLocation);
            }

            NavigationService.GoBack();        
		}
    }
}