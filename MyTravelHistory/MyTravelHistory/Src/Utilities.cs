using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using FlurryWP8SDK;
using Windows.Devices.Geolocation;
using Microsoft.Phone.Maps.Services;
using System.Device.Location;
using MyTravelHistory.Models;

namespace MyTravelHistory.Src
{
    public class Utilities
    {
        public static string GetVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().FullName.Split('=')[1].Split(',')[0];
        }

        public static byte[] ConvertToBytes(WriteableBitmap image)
        {
            MemoryStream ms = new MemoryStream();
            image.SaveJpeg(ms, 1000, 2000, 0, 100);

            return ms.GetBuffer();
        }

        public static WriteableBitmap ConvertToImage(byte[] inputBytes)
        {
            return GetImage(inputBytes, 1000, 2000);
        }

        public static WriteableBitmap ConvertToImage(byte[] inputBytes, int width, int height)
        {
            return GetImage(inputBytes, width, height);
        }

        private static WriteableBitmap GetImage(byte[] inputBytes, int width, int height)
        {
            WriteableBitmap img = new WriteableBitmap(width, height);

            var ms = new MemoryStream(inputBytes);
            img.LoadJpeg(ms);

            return img;
        }

        public static async Task GetPosition()
        {
            Geolocator geolocater = new Geolocator();
            geolocater.DesiredAccuracy = PositionAccuracy.High;

            try
            {
                Geoposition geoposition = await geolocater.GetGeopositionAsync(
                    maximumAge: TimeSpan.FromMinutes(2),
                    timeout: TimeSpan.FromSeconds(30)
                    );

                App.ViewModel.CurrentPosition = new Position();
                App.ViewModel.CurrentPosition.Latitude = geoposition.Coordinate.Latitude;
                App.ViewModel.CurrentPosition.Longitude = geoposition.Coordinate.Longitude;
                App.ViewModel.CurrentPosition.Accuracy = geoposition.Coordinate.Accuracy;
                App.ViewModel.CurrentPosition.Timestamp = Convert.ToDateTime(geoposition.Coordinate.Timestamp);
            }
            catch (Exception ex)
            {
                if ((uint)ex.HResult == 0x80004004)
                {
                    // the application does not have the right capability or the location master switch is off
                    Api.LogError("location  is disabled in phone settings.", ex.InnerException);
                }
            }
        }

        public static async Task<LocationAddress> GetAddress(double latitude, double longtitude)
        {
            ReverseGeocodeQuery myReverseGeocodeQuery = new ReverseGeocodeQuery();
            myReverseGeocodeQuery.GeoCoordinate = new GeoCoordinate(latitude, longtitude);
            IList<MapLocation> locations = await myReverseGeocodeQuery.GetMapLocationsAsync();
            LocationAddress locationAddress = new LocationAddress();

            if (locations.Count > 0)
            {
                MapAddress address = locations.First<MapLocation>().Information.Address;
                locationAddress = new LocationAddress()
                {
                    Street = address.Street,
                    HouseNumber = address.HouseNumber,
                    PostalCode = address.PostalCode,
                    City = address.City,
                    District = address.District,
                    State = address.State,
                    Country = address.Country
                };
            }

            return locationAddress;
        }
    }
}
