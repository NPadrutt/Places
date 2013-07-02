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

            await FetchCurrentPosition();
            PinCurrentPosition();
        }

        private void PinCurrentPosition()
        {
            var currentPosition = new GeoCoordinate(App.ViewModel.CurrentPosition.Latitude, App.ViewModel.CurrentPosition.Longitude);
            MyMap.SetView(currentPosition, 16, MapAnimationKind.Parabolic);
            
            var marker = new UserLocationMarker(){ GeoCoordinate = currentPosition };
            var mapOverlay = new MapOverlay() {Content = marker, GeoCoordinate = currentPosition};

            var mapLayer = new MapLayer { mapOverlay };
            MyMap.Layers.Add(mapLayer);
        }

        private void PinMap(GeoCoordinate geoPosition, string locationName)
        {
            var mapOverlay = new MapOverlay();
            var pin = new Pushpin()
            {
                Content = locationName
            };
            mapOverlay.Content = pin;
            mapOverlay.GeoCoordinate = geoPosition;

            var mapLayer = new MapLayer { mapOverlay };
            MyMap.Layers.Add(mapLayer);
        }

        private async void btnNavigation_Click(object sender, EventArgs e)
        {            
            this.mygeocodequery = new GeocodeQuery { SearchTerm = App.ViewModel.SelectedLocation.Name };

            await FetchCurrentPosition();          

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
            if (App.ViewModel.CurrentPosition == null || App.ViewModel.CurrentPosition.Timestamp <= DateTime.Now.AddMinutes(1))
            {
                busyProceedAction.IsRunning = true;
                await Utilities.GetPosition();
                busyProceedAction.IsRunning = false;
            }
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