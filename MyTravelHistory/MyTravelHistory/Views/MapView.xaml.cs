using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Device.Location;
using Microsoft.Phone.Maps.Controls;
using MyTravelHistory.Models;
using Microsoft.Phone.Maps.Toolkit;
using Microsoft.Phone.Maps.Services;
using Windows.Devices.Geolocation;
using FlurryWP8SDK;
using MyTravelHistory.Src;
using System.Threading.Tasks;
using MyTravelHistory.Resources;

namespace MyTravelHistory.Views
{
    public partial class MapView : PhoneApplicationPage
    {
        private bool MultipleLocations;

        List<GeoCoordinate> MyCoordinates = new List<GeoCoordinate>();
        RouteQuery MyQuery = null;
        GeocodeQuery Mygeocodequery = null;

        public MapView()
        {
            InitializeComponent();

            (ApplicationBar.Buttons[0] as ApplicationBarIconButton).Text = AppResources.NavigateLabel;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (this.NavigationContext.QueryString != null || this.NavigationContext.QueryString.Count > 0)
            {
                if (Convert.ToBoolean(this.NavigationContext.QueryString["MultipleLocations"]))
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
        }

        private async void Map_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Microsoft.Phone.Maps.MapsSettings.ApplicationContext.ApplicationId = "ApplicationID";
            Microsoft.Phone.Maps.MapsSettings.ApplicationContext.AuthenticationToken = "AuthenticationToken";

            if (MultipleLocations)
            {
                foreach (Location location in App.ViewModel.SelectedLocations)
                {
                    PinMap(new GeoCoordinate(location.Latitude, location.Longitude), location.Name);
                }
            }
            else
            {
                PinMap(new GeoCoordinate(App.ViewModel.SelectedLocation.Latitude, App.ViewModel.SelectedLocation.Longitude), App.ViewModel.SelectedLocation.Name);
            }

            await FetchCurrentPosition();
            PinMap(new GeoCoordinate(App.ViewModel.CurrentPosition.Latitude, App.ViewModel.CurrentPosition.Longitude), AppResources.YouAreHereText);
        }

        public void PinMap(GeoCoordinate geoPosition, string Name)
        {
            MyMap.Center = geoPosition;
            MyMap.ZoomLevel = 16;

            var mapOverlay = new MapOverlay();
            Pushpin pin = new Pushpin()
            {
                Content = Name
            };
            mapOverlay.Content = pin;
            mapOverlay.GeoCoordinate = geoPosition;

            var mapLayer = new MapLayer();
            mapLayer.Add(mapOverlay);

            MyMap.Layers.Add(mapLayer);
        }

        private async void btnNavigation_Click(object sender, System.EventArgs e)
        {            
            Mygeocodequery = new GeocodeQuery();
            Mygeocodequery.SearchTerm = App.ViewModel.SelectedLocation.Name;

            await FetchCurrentPosition();          

            Mygeocodequery.GeoCoordinate = new GeoCoordinate(App.ViewModel.CurrentPosition.Latitude, App.ViewModel.CurrentPosition.Longitude);
            MyCoordinates.Add(Mygeocodequery.GeoCoordinate);

            MyQuery = new RouteQuery();
            MyCoordinates.Add(new GeoCoordinate(App.ViewModel.SelectedLocation.Latitude, App.ViewModel.SelectedLocation.Longitude));
            MyQuery.Waypoints = MyCoordinates;
            MyQuery.QueryCompleted += MyQuery_QueryCompleted;
            MyQuery.QueryAsync();
            Mygeocodequery.Dispose();
            
        }

        private async Task FetchCurrentPosition()
        {
            if (App.ViewModel.CurrentPosition == null && App.ViewModel.CurrentPosition.Timestamp <= DateTime.Now.AddMinutes(1))
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
                Route MyRoute = e.Result;
                MapRoute MyMapRoute = new MapRoute(MyRoute);
                MyMap.AddRoute(MyMapRoute);
                MyQuery.Dispose();
            }

        }
    }
}