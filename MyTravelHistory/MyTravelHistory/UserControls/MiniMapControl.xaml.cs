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
        public MiniMapControl()
        {
            InitializeComponent();
        }

        public void ShowOnMap(double latitude, double longitude)
        {
            var geoPosition = new GeoCoordinate(latitude, longitude);
            myMap.SetView(geoPosition, 18, MapAnimationKind.Parabolic);

            var mapOverlay = new MapOverlay();
            var userMarker = new UserLocationMarker();
            mapOverlay.Content = userMarker;
            mapOverlay.GeoCoordinate = geoPosition;

            var mapLayer = new MapLayer { mapOverlay };
            myMap.Layers.Add(mapLayer);
        }      
    }
}
