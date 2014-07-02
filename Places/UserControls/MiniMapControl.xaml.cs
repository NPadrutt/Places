using Microsoft.Phone.Maps;
using Microsoft.Phone.Maps.Controls;
using Microsoft.Phone.Maps.Toolkit;
using System.Device.Location;

namespace Places.UserControls
{
    public partial class MiniMapControl
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