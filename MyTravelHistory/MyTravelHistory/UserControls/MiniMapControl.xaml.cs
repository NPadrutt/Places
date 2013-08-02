using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Maps.Toolkit;
using Microsoft.Phone.Maps.Controls;
using System.Device.Location;
using MyTravelHistory.Resources;

namespace MyTravelHistory.UserControls
{
    public partial class MiniMapControl : UserControl
    {
        private MapOverlay mapOverlay;
        private UserLocationMarker userMarker;

        public MiniMapControl()
        {
            InitializeComponent();
        }

        public void ShowOnMap(double latitude, double longitude)
        {
            var geoPosition = new GeoCoordinate(latitude, longitude);
            myMap.SetView(geoPosition, 18, MapAnimationKind.Parabolic);

            mapOverlay = new MapOverlay();
            userMarker = new UserLocationMarker();
            mapOverlay.Content = userMarker;
            mapOverlay.GeoCoordinate = geoPosition;

            var mapLayer = new MapLayer { mapOverlay };
            myMap.Layers.Add(mapLayer);
        }

        public void ClearPushPins()
        {
            myMap.Layers.Clear();
        }

        private void myMap_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            MapsSettings.ApplicationContext.ApplicationId = "69550432-a2f9-490f-a782-d4c91775382e";
            MapsSettings.ApplicationContext.AuthenticationToken = "5-Sp7gaotYB4nRJsslF5JQ";
        }
    }
}
