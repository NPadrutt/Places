using Microsoft.Phone.Controls;
using Microsoft.Phone.Maps;
using Microsoft.Phone.Maps.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using MyTravelHistory.Resources;
using MyTravelHistory.Src;
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Phone.Maps.Toolkit;


namespace MyTravelHistory.Views
{
    public partial class MapView : PhoneApplicationPage
    {
        readonly List<GeoCoordinate> myCoordinates = new List<GeoCoordinate>();
        private GeoCoordinate selectedCoordinate;

        private UserLocationMarker marker;
        private MapOverlay mapOverlay;

        public MapView()
        {
            InitializeComponent();

            ((ApplicationBarIconButton)this.ApplicationBar.Buttons[0]).Text = AppResources.NavigateLabel;
            ((ApplicationBarIconButton)this.ApplicationBar.Buttons[1]).Text = AppResources.SwapPositionLabel;

            ((ApplicationBarMenuItem)this.ApplicationBar.MenuItems[0]).Text = AppResources.RefreshCurrentPositionLabel;
            ((ApplicationBarMenuItem)this.ApplicationBar.MenuItems[1]).Text = AppResources.DownloadMapsLabel;
        }

        private void Map_Loaded(object sender, RoutedEventArgs e)
        {
            MapsSettings.ApplicationContext.ApplicationId = "69550432-a2f9-490f-a782-d4c91775382e";
            MapsSettings.ApplicationContext.AuthenticationToken = "5-Sp7gaotYB4nRJsslF5JQ";

            PinMap(new GeoCoordinate(App.ViewModel.SelectedLocation.Latitude, App.ViewModel.SelectedLocation.Longitude), App.ViewModel.SelectedLocation.Name);

            GetCurrentPosition(false);
        }

        private void PinCurrentPosition()
        {
            var currentPosition = new GeoCoordinate(App.ViewModel.CurrentPosition.Latitude, App.ViewModel.CurrentPosition.Longitude);
            
            this.marker = new UserLocationMarker(){ GeoCoordinate = currentPosition };
            this.mapOverlay = new MapOverlay() {Content = this.marker, GeoCoordinate = currentPosition};

            var mapLayer = new MapLayer { this.mapOverlay };
            MyMap.Layers.Add(mapLayer);
        }

        private void PinMap(GeoCoordinate geoPosition, string locationName)
        {
            MyMap.SetView(geoPosition, 16, MapAnimationKind.Parabolic);
            var mapOverlayPin = new MapOverlay();
            var pin = new Pushpin()
            {
                Content = locationName
            };
            mapOverlayPin.Content = pin;
            mapOverlayPin.GeoCoordinate = geoPosition;

            var mapLayer = new MapLayer { mapOverlayPin };
            MyMap.Layers.Add(mapLayer);

            this.selectedCoordinate = geoPosition;
        }

        private async void btnNavigation_Click(object sender, EventArgs e)
        {            
            var bingMapsDirectionsTask = new BingMapsDirectionsTask();
            var location = new LabeledMapLocation(App.ViewModel.SelectedLocation.Name, new GeoCoordinate(App.ViewModel.SelectedLocation.Latitude, App.ViewModel.SelectedLocation.Longitude));
            bingMapsDirectionsTask.End = location;

            bingMapsDirectionsTask.Show();
        }

        private void menuRefreshPosition_Click(object sender, System.EventArgs e)
        {
            GetCurrentPosition(true);
        }

        private async Task GetCurrentPosition(bool refetch)
        {
            if (App.ViewModel.CurrentPosition == null || App.ViewModel.CurrentPosition.Timestamp >= DateTime.Now.AddMinutes(1) || refetch)
            {
                progressionbarGetLocation.IsEnabled = true;
                progressionbarGetLocation.Visibility = Visibility.Visible;
                lblStatus.Visibility = Visibility.Visible;

                await Utilities.GetPosition();
            }
            PinCurrentPosition();
            progressionbarGetLocation.IsEnabled = false;
            progressionbarGetLocation.Visibility = Visibility.Collapsed;
            lblStatus.Visibility = Visibility.Collapsed;
        }

        private void menuDownloadMaps_Click(object sender, System.EventArgs e)
        {
            var mapDownloaderTask = new MapDownloaderTask();
            mapDownloaderTask.Show();
        }

        private void btnSwapPosition_Click(object sender, System.EventArgs e)
        {
            if (App.ViewModel.CurrentPosition == null)
            {
                MessageBox.Show(AppResources.NoPositionMessage, AppResources.NoPositionMessageTitle, MessageBoxButton.OK);
                return;
            }

            var currentCoordinate = new GeoCoordinate(App.ViewModel.CurrentPosition.Latitude, App.ViewModel.CurrentPosition.Longitude);
            this.selectedCoordinate = this.selectedCoordinate == currentCoordinate ? new GeoCoordinate(App.ViewModel.SelectedLocation.Latitude, App.ViewModel.SelectedLocation.Longitude) : currentCoordinate;
            MyMap.SetView(this.selectedCoordinate, 16, MapAnimationKind.Parabolic);
        }
    }
}