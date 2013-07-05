using System.ServiceModel.Channels;
using Windows.Devices.Geolocation;
using FlurryWP8SDK;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Maps;
using Microsoft.Phone.Maps.Controls;
using Microsoft.Phone.Maps.Services;
using Microsoft.Phone.Shell;
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

        private async void Map_Loaded(object sender, RoutedEventArgs e)
        {
            MapsSettings.ApplicationContext.ApplicationId = "ApplicationID";
            MapsSettings.ApplicationContext.AuthenticationToken = "AuthenticationToken";

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

            var geolocator = new Geolocator() {MovementThreshold = 10, DesiredAccuracy = PositionAccuracy.High};
            geolocator.StatusChanged += geolocator_StatusChanged;
            geolocator.PositionChanged += geolocator_PositionChanged;
        }

        private void geolocator_StatusChanged(Geolocator sender, StatusChangedEventArgs args)
        {
            string status = "";

            switch (args.Status)
            {
                case PositionStatus.Disabled:
                    // the application does not have the right capability or the location master switch is off
                    status = "location is disabled in phone settings";
                    break;
                case PositionStatus.Initializing:
                    // the geolocator started the tracking operation
                    status = "initializing";
                    break;
                case PositionStatus.NoData:
                    // the location service was not able to acquire the location
                    status = "no data";
                    break;
                case PositionStatus.Ready:
                    // the location service is generating geopositions as specified by the tracking parameters
                    status = "ready";
                    break;
                case PositionStatus.NotAvailable:
                    status = "not available";
                    // not used in WindowsPhone, Windows desktop uses this value to signal that there is no hardware capable to acquire location information
                    break;
                case PositionStatus.NotInitialized:
                    // the initial state of the geolocator, once the tracking operation is stopped by the user the geolocator moves back to this state
                    break;
            }

            Api.LogError(status, null);
        }


        private void geolocator_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            Dispatcher.BeginInvoke(() =>
            {
                marker.GeoCoordinate = args.Position.Coordinate.ToGeoCoordinate();
                mapOverlay.GeoCoordinate = args.Position.Coordinate.ToGeoCoordinate();
            });
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
            this.mygeocodequery = new GeocodeQuery { SearchTerm = App.ViewModel.SelectedLocation.Name };

            busyProceedAction.IsRunning = true;
            await FetchCurrentPosition();
            busyProceedAction.IsRunning = false;

            this.mygeocodequery.GeoCoordinate = new GeoCoordinate(App.ViewModel.CurrentPosition.Latitude, App.ViewModel.CurrentPosition.Longitude);
            MyCoordinates.Add(this.mygeocodequery.GeoCoordinate);

            this.myQuery = new RouteQuery();
            MyCoordinates.Add(new GeoCoordinate(App.ViewModel.SelectedLocation.Latitude, App.ViewModel.SelectedLocation.Longitude));
            this.myQuery.Waypoints = MyCoordinates;
            this.myQuery.QueryCompleted += MyQuery_QueryCompleted;
            this.myQuery.QueryAsync();
            this.mygeocodequery.Dispose();
            
        }

        private async Task FetchCurrentPosition()
        {
            if (App.ViewModel.CurrentPosition == null || App.ViewModel.CurrentPosition.Timestamp >= DateTime.Now.AddMinutes(1))
            {
                await Utilities.GetPosition();
            }
            PinCurrentPosition();
        }

        void MyQuery_QueryCompleted(object sender, QueryCompletedEventArgs<Route> e)
        {
            if (e.Error == null)
            {
                Route myRoute = e.Result;
                var myMapRoute = new MapRoute(myRoute);
                MyMap.AddRoute(myMapRoute);
                this.myQuery.Dispose();
            }

        }
    }
}