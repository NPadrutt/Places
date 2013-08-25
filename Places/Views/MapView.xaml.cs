using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Maps;
using Microsoft.Phone.Maps.Controls;
using Microsoft.Phone.Maps.Toolkit;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Places.Resources;
using Places.Src;

namespace Places.Views
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

            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = AppResources.SwapPositionLabel;
            ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).Text = AppResources.NavigateLabel;

            ((ApplicationBarMenuItem)ApplicationBar.MenuItems[0]).Text = AppResources.RefreshCurrentPositionLabel;
            ((ApplicationBarMenuItem)ApplicationBar.MenuItems[1]).Text = AppResources.DownloadMapsLabel;
        }

        private async void Map_Loaded(object sender, RoutedEventArgs e)
        {
            MapsSettings.ApplicationContext.ApplicationId = "69550432-a2f9-490f-a782-d4c91775382e";
            MapsSettings.ApplicationContext.AuthenticationToken = "5-Sp7gaotYB4nRJsslF5JQ";

            PinMap(new GeoCoordinate(App.ViewModel.SelectedLocation.Latitude, App.ViewModel.SelectedLocation.Longitude), App.ViewModel.SelectedLocation.Name);

            await GetCurrentPosition(false);
        }

        private void PinCurrentPosition()
        {
            var currentPosition = new GeoCoordinate(App.ViewModel.CurrentPosition.Latitude, App.ViewModel.CurrentPosition.Longitude);
            
            marker = new UserLocationMarker(){ GeoCoordinate = currentPosition };
            mapOverlay = new MapOverlay() {Content = this.marker, GeoCoordinate = currentPosition};

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

            selectedCoordinate = geoPosition;
        }

        private void btnNavigation_Click(object sender, EventArgs e)
        {            
            var bingMapsDirectionsTask = new BingMapsDirectionsTask();
            var location = new LabeledMapLocation(App.ViewModel.SelectedLocation.Name, new GeoCoordinate(App.ViewModel.SelectedLocation.Latitude, App.ViewModel.SelectedLocation.Longitude));
            bingMapsDirectionsTask.End = location;

            bingMapsDirectionsTask.Show();
        }

        private async void menuRefreshPosition_Click(object sender, EventArgs e)
        {
            await GetCurrentPosition(true);
        }

        private async Task GetCurrentPosition(bool refetch)
        {
            if (App.ViewModel.CurrentPosition == null || App.ViewModel.CurrentPosition.Timestamp >= DateTime.Now.AddMinutes(1) || refetch)
            {
                ProgressionbarGetLocation.IsVisible = true;
                await Utilities.GetPosition();
            }
            PinCurrentPosition();
            ProgressionbarGetLocation.IsVisible = false;
        }

        private void menuDownloadMaps_Click(object sender, EventArgs e)
        {
            var mapDownloaderTask = new MapDownloaderTask();
            mapDownloaderTask.Show();
        }

        private void btnSwapPosition_Click(object sender, EventArgs e)
        {
            if (App.ViewModel.CurrentPosition == null)
            {
                MessageBox.Show(AppResources.NoPositionMessage, AppResources.NoPositionMessageTitle, MessageBoxButton.OK);
                return;
            }

            var currentCoordinate = new GeoCoordinate(App.ViewModel.CurrentPosition.Latitude, App.ViewModel.CurrentPosition.Longitude);
            selectedCoordinate = selectedCoordinate == currentCoordinate ? new GeoCoordinate(App.ViewModel.SelectedLocation.Latitude, App.ViewModel.SelectedLocation.Longitude) : currentCoordinate;
            MyMap.SetView(selectedCoordinate, 16, MapAnimationKind.Parabolic);
        }

        private void btnLayers_Click(object sender, EventArgs e)
        {
            WindowLayers.IsOpen = !WindowLayers.IsOpen;
            
            switch (MyMap.CartographicMode)
            {
                case MapCartographicMode.Road:
                    RadioBtnRoad.IsChecked = true;
                    break;
                case MapCartographicMode.Aerial:
                    RadioBtnAerial.IsChecked = true;
                    break;
                case MapCartographicMode.Hybrid:
                    RadioBtnHybrid.IsChecked = true;
                    break;
                case MapCartographicMode.Terrain:
                    RadioBtnTerrain.IsChecked = true;
                    break;
            }
        }

        private void RadioBtnRoad_Checked(object sender, RoutedEventArgs e)
        {
            MyMap.CartographicMode = MapCartographicMode.Road;
        }

        private void RadioBtnAerial_Checked(object sender, RoutedEventArgs e)
        {
            MyMap.CartographicMode = MapCartographicMode.Aerial;
        }

        private void RadioBtnHybrid_Checked(object sender, RoutedEventArgs e)
        {
            MyMap.CartographicMode = MapCartographicMode.Hybrid;
        }

        private void RadioBtnTerrain_Checked(object sender, RoutedEventArgs e)
        {
            MyMap.CartographicMode = MapCartographicMode.Terrain;
        }

    }
}