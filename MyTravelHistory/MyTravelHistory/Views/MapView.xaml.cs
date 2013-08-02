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
        private bool MultipleLocations;

        readonly List<GeoCoordinate> MyCoordinates = new List<GeoCoordinate>();
        RouteQuery myQuery = null;
        GeocodeQuery mygeocodequery = null;

        private UserLocationMarker marker;
        private MapOverlay mapOverlay;

        public MapView()
        {
            InitializeComponent();

            ((ApplicationBarIconButton)this.ApplicationBar.Buttons[0]).Text = AppResources.NavigateLabel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (NavigationContext.QueryString == null || NavigationContext.QueryString.Count <= 0) return;
            if (Convert.ToBoolean(NavigationContext.QueryString["MultipleLocations"]))
            {
                MultipleLocations = true;
                ApplicationBar.IsVisible = false;
            }
            else
            {
                MultipleLocations = false;
                ApplicationBar.IsVisible = false;
            }
        }

        private void Map_Loaded(object sender, RoutedEventArgs e)
        {
            MapsSettings.ApplicationContext.ApplicationId = "69550432-a2f9-490f-a782-d4c91775382e";
            MapsSettings.ApplicationContext.AuthenticationToken = "5-Sp7gaotYB4nRJsslF5JQ";

            if (MultipleLocations)
            {
                foreach (var location in App.ViewModel.SelectedLocations)
                {
                    PinMap(new GeoCoordinate(location.Latitude, location.Longitude), location.Name);
                }
            }
            else
            {
                PinMap(new GeoCoordinate(App.ViewModel.SelectedLocation.Latitude, App.ViewModel.SelectedLocation.Longitude), App.ViewModel.SelectedLocation.Name);
            }

            FetchCurrentPosition();
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

        private async Task FetchCurrentPosition()
        {
            if (App.ViewModel.CurrentPosition == null || App.ViewModel.CurrentPosition.Timestamp >= DateTime.Now.AddMinutes(1))
            {
                progressionbarGetLocation.IsEnabled = true;
                lblStatus.Visibility = Visibility.Visible;

                await Utilities.GetPosition();
            }
            PinCurrentPosition();
            progressionbarGetLocation.IsEnabled = false;
            lblStatus.Visibility = Visibility.Collapsed;
        }
    }
}