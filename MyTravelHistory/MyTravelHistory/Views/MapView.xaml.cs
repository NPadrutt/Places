using System.ServiceModel.Channels;
using Windows.Devices.Geolocation;
using FlurryWP8SDK;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Maps;
using Microsoft.Phone.Maps.Controls;
using Microsoft.Phone.Maps.Services;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using MyTravelHistory.Resources;
using MyTravelHistory.Src;
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Maps.Toolkit;


namespace MyTravelHistory.Views
{
    public partial class MapView : PhoneApplicationPage
    {
        readonly List<GeoCoordinate> MyCoordinates = new List<GeoCoordinate>();

        private UserLocationMarker marker;
        private MapOverlay mapOverlay;

        public MapView()
        {
            InitializeComponent();

            ((ApplicationBarIconButton)this.ApplicationBar.Buttons[0]).Text = AppResources.NavigateLabel;

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
            
            marker = new UserLocationMarker(){ GeoCoordinate = currentPosition };
            mapOverlay = new MapOverlay() {Content = marker, GeoCoordinate = currentPosition};

            var mapLayer = new MapLayer { mapOverlay };
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
            MapDownloaderTask mapDownloaderTask = new MapDownloaderTask();
            mapDownloaderTask.Show();
        }
    }
}