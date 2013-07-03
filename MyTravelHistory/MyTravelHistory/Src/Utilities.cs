using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using FlurryWP8SDK;
using Windows.Devices.Geolocation;
using Microsoft.Phone.Maps.Services;
using System.Device.Location;
using MyTravelHistory.Models;
using Telerik.Windows.Controls;
using System.IO.IsolatedStorage;

namespace MyTravelHistory.Src
{
    public class Utilities
    {
        public static string GetVersion()
        {
            return Assembly.GetExecutingAssembly().FullName.Split('=')[1].Split(',')[0];
        }

        public static byte[] ConvertToBytes(WriteableBitmap image)
        {
            var ms = new MemoryStream();
            image.SaveJpeg(ms, image.PixelWidth, image.PixelHeight, 0, 20);

            return ms.GetBuffer();
        }

        public static WriteableBitmap ConvertToImage(byte[] inputBytes)
        {
            return GetImage(inputBytes, 3264, 2448);
        }

        public static WriteableBitmap ConvertToImage(byte[] inputBytes, int width, int height)
        {
            return GetImage(inputBytes, width, height);
        }

        private static WriteableBitmap GetImage(byte[] inputBytes, int width, int height)
        {
            var img = new WriteableBitmap(width, height);

            var ms = new MemoryStream(inputBytes);
            img.LoadJpeg(ms);

            return img;
        }

        public static async Task GetPosition()
        {
            var geolocater = new Geolocator { DesiredAccuracy = PositionAccuracy.High };

            try
            {
                var geoposition = await geolocater.GetGeopositionAsync(
                    maximumAge: TimeSpan.FromMinutes(2),
                    timeout: TimeSpan.FromSeconds(30)
                    );

                App.ViewModel.CurrentPosition = new Position
                {
                    Latitude = geoposition.Coordinate.Latitude,
                    Longitude = geoposition.Coordinate.Longitude,
                    Accuracy = geoposition.Coordinate.Accuracy,
                    Timestamp = geoposition.Coordinate.Timestamp.DateTime
                };
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
            var myReverseGeocodeQuery = new ReverseGeocodeQuery
            {
                GeoCoordinate = new GeoCoordinate(latitude, longtitude)
            };
            IList<MapLocation> locations = await myReverseGeocodeQuery.GetMapLocationsAsync();
            var locationAddress = new LocationAddress();

            if (locations.Count <= 0) return locationAddress;

            var address = locations.First().Information.Address;
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

            return locationAddress;
        }

        public static void CreateTile()
        {
            var tileData = new RadExtendedTileData()
            {
                Title = App.ViewModel.SelectedLocation.Name
            };

            LiveTileHelper.CreateOrUpdateTile(tileData, new Uri("/Views/DetailsLocation.xaml?id=" + App.ViewModel.SelectedLocation.Id, UriKind.RelativeOrAbsolute));
        }

        public static string SaveImageToLocalStorage(byte[] imageBytes)
        {
            const string DirectoryName = "thumbnails";
            string imageName = new Guid().ToString();
            string path = DirectoryName + "\\" + imageName;

            using (var myIsoStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!myIsoStorage.DirectoryExists(DirectoryName))
                {
                    myIsoStorage.CreateDirectory(DirectoryName);
                }

                if (myIsoStorage.FileExists(path))
                {
                    myIsoStorage.DeleteFile(imageName);
                }
                IsolatedStorageFileStream fileStream = myIsoStorage.CreateFile(path);
                WriteableBitmap mywb = ConvertToImage(imageBytes);
                mywb.SaveJpeg(fileStream, mywb.PixelWidth, mywb.PixelHeight, 0, 95);
                fileStream.Close();
            }

            return imageName;
        }
    }
}
