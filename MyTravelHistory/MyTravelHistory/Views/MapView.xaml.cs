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

namespace MyTravelHistory.Views
{
    public partial class MapView : PhoneApplicationPage
    {
        private bool MultipleLocations;

        public MapView()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (this.NavigationContext.QueryString != null && this.NavigationContext.QueryString.Count > 0)
            {
                if (Convert.ToBoolean(this.NavigationContext.QueryString["MultipleLocations"]))
                {
                    MultipleLocations = true;
                }
                else
                {
                    MultipleLocations = false;
                }
            }
        }

        private void Map_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Microsoft.Phone.Maps.MapsSettings.ApplicationContext.ApplicationId = "ApplicationID";
            Microsoft.Phone.Maps.MapsSettings.ApplicationContext.AuthenticationToken = "AuthenticationToken";

            if (MultipleLocations)
            {
                foreach (Location location in App.ViewModel.SelectedLocations)
                {
                    PinMap(new GeoCoordinate(location.Latitude, location.Longtitude), location.Name);
                }
            }
            else
            {
                PinMap(new GeoCoordinate(App.ViewModel.SelectedLocation.Latitude, App.ViewModel.SelectedLocation.Longtitude), App.ViewModel.SelectedLocation.Name);
            }
        }

        public void PinMap(GeoCoordinate geoPosition, string Name)
        {
            MyMap.Center = geoPosition;
            MyMap.ZoomLevel = 13;

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
    }
}