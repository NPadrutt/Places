using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Maps.Toolkit;
using Microsoft.Phone.Maps.Controls;
using System.Device.Location;

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
            myMap.Center = geoPosition;
            myMap.ZoomLevel = 18;

            var mapOverlay = new MapOverlay();
            var pin = new Pushpin()
            {
                Content = Name
            };
            mapOverlay.Content = pin;
            mapOverlay.GeoCoordinate = geoPosition;

            var mapLayer = new MapLayer { mapOverlay };
            myMap.Layers.Add(mapLayer);
        }      
    }
}
