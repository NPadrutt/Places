using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.Devices.Geolocation;

namespace MyTravelHistory.Src
{
    public class PositionHelper
    {
        public async void GetPosition()
        {
            Geolocator geolocater = new Geolocator();
            geolocater.DesiredAccuracy = PositionAccuracy.High;

            try
            {
                Geoposition geoposition = await geolocater.GetGeopositionAsync(
                    maximumAge: TimeSpan.FromMinutes(2),
                    timeout: TimeSpan.FromSeconds(10)
                    );

                App.ViewModel.SelectedLocations.Latitude = geoposition.Coordinate.Latitude.ToString();
                App.ViewModel.SelectedLocations.Longtitude = geoposition.Coordinate.Longitude.ToString();
            }
            catch (Exception ex)
            {
                if ((uint)ex.HResult == 0x80004004)
                {
                    // the application does not have the right capability or the location master switch is off
                    MessageBox.Show("location  is disabled in phone settings.");
                }
            }
        }
    }
}
