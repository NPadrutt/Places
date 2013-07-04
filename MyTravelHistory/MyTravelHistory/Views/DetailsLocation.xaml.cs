using System;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using MyTravelHistory.Resources;
using MyTravelHistory.Src;
using Telerik.Windows.Controls;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace MyTravelHistory.Views
{
    public partial class DetailsLocation : PhoneApplicationPage
    {
        public DetailsLocation()
        {
            InitializeComponent();

            DataContext = App.ViewModel.SelectedLocation;

            ((ApplicationBarIconButton)this.ApplicationBar.Buttons[0]).Text = AppResources.EditLabel;
            
			((ApplicationBarMenuItem)this.ApplicationBar.MenuItems[0]).Text = AppResources.PintToStartLabel;
			((ApplicationBarMenuItem)this.ApplicationBar.MenuItems[1]).Text = AppResources.DeleteLabel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (this.NavigationContext.QueryString == null || this.NavigationContext.QueryString.Count <= 0) return;
            
            //Readout Querystrings
            if (this.NavigationContext.QueryString.ContainsKey("RemoveBackstack") && Convert.ToBoolean(this.NavigationContext.QueryString["RemoveBackstack"]))
            {
                this.NavigationService.RemoveBackEntry();
            }

            if (this.NavigationContext.QueryString.ContainsKey("id"))
            {
                foreach (var location in App.ViewModel.AllLocations.Where(location => location.Id == Convert.ToInt32(this.NavigationContext.QueryString["id"])))
                {
                    App.ViewModel.SelectedLocation = location;
                }
            }

            MiniMap.ShowOnMap(App.ViewModel.SelectedLocation.Latitude, App.ViewModel.SelectedLocation.Longitude);
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
            if (App.ViewModel.SelectedLocation.LocationImageName != null)
            {
                LocationImage.Source = Utilities.LoadLocationImage();
            }
        }

        private void LocationImageLarge_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.ViewModel.SelectedLocation.LocationImageName != null)
            {
                locationImageLarge.Source = LocationImage.Source;
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

        private void mPinToStart_Click(object sender, EventArgs e)
        {
            var tileData = new RadExtendedTileData()
            {
                Title = App.ViewModel.SelectedLocation.Name,
                VisualElement = LocationImage,
                IsTransparencySupported = true
            };

            LiveTileHelper.CreateOrUpdateTile(tileData, new Uri("/Views/DetailsLocation.xaml?id=" + App.ViewModel.SelectedLocation.Id, UriKind.RelativeOrAbsolute));
        }
    }
}