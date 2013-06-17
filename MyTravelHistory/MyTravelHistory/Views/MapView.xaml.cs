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

namespace MyTravelHistory.Views
{
    public partial class MapView : PhoneApplicationPage
    {
        public MapView()
        {
            InitializeComponent();
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