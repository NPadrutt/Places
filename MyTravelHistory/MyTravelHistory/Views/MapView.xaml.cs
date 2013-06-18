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
using Microsoft.Phone.Maps.Toolkit;

namespace MyTravelHistory.Views
{
    public partial class MapView : PhoneApplicationPage
    {
        public MapView()
        {
            InitializeComponent();

            MapLayer layer = new MapLayer();

            Pushpin pushpin = new Pushpin();

            pushpin.GeoCoordinate = new GeoCoordinate(App.ViewModel.SelectedLocations.Latitude, App.ViewModel.SelectedLocations.Longtitude);
            MapOverlay overlay = new MapOverlay();
            overlay.Content = pushpin;
            overlay.GeoCoordinate = new GeoCoordinate(App.ViewModel.SelectedLocations.Latitude, App.ViewModel.SelectedLocations.Longtitude);
            layer.Add(overlay);

            Map.Layers.Add(layer);
        }

        private void Map_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Microsoft.Phone.Maps.MapsSettings.ApplicationContext.ApplicationId = "ApplicationID";
            Microsoft.Phone.Maps.MapsSettings.ApplicationContext.AuthenticationToken = "AuthenticationToken";

            Map.Center = new GeoCoordinate(App.ViewModel.SelectedLocations.Latitude, App.ViewModel.SelectedLocations.Longtitude);
            Map.ZoomLevel = 15;
        }
    }
}