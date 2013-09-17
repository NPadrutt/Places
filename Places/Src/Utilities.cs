using System;
using System.Collections.Generic;
using System.Device.Location;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Windows.Devices.Geolocation;
using ExifLib;
using FlurryWP8SDK;
using Microsoft.Phone.Maps.Services;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework.Media;
using Places.Models;
using Places.Resources;
using Telerik.Windows.Controls;

namespace Places.Src
{
    public static class Utilities
    {
        public static string GetVersion()
        {
            return Assembly.GetExecutingAssembly().FullName.Split('=')[1].Split(',')[0];
        }

        public static async Task GetPosition()
        {
            if (App.Settings.LocationServiceEnabled)
            {
                var geolocater = new Geolocator {DesiredAccuracy = PositionAccuracy.High};

                try
                {
                    var geoposition =
                        await geolocater.GetGeopositionAsync(TimeSpan.FromMinutes(2), TimeSpan.FromSeconds(60)
                                  );

                    App.ViewModel.CurrentPosition = new Position
                        {
                            Latitude = geoposition.Coordinate.Latitude,
                            Longitude = geoposition.Coordinate.Longitude,
                            Accuracy = geoposition.Coordinate.Accuracy,
                            Timestamp = geoposition.Coordinate.Timestamp.DateTime
                        };
                }
                catch (TimeoutException ex)
                {
                    Api.LogError("timeout", ex);
                }
                catch (Exception ex)
                {
                    if ((uint) ex.HResult == 0x80004004)
                    {
                        // the application does not have the right capability or the location master switch is off
                        Api.LogError("location  is disabled in phone settings.", ex);
                    }
                    if ((uint) ex.HResult == 0x800705B4)
                    {
                        // the application does not have the right capability or the location master switch is off
                        Api.LogError("Timeout expired.", ex);
                    }
                }
            }
        }

        public static async Task GetAddress(double latitude, double longtitude)
        {
            var locationAddress = new LocationAddress();
            try
            {
                var myReverseGeocodeQuery = new ReverseGeocodeQuery
                    {
                        GeoCoordinate = new GeoCoordinate(latitude, longtitude)
                    };
                IList<MapLocation> locations = await myReverseGeocodeQuery.GetMapLocationsAsync();


                if (locations.Count > 0)
                {
                    var address = locations.First().Information.Address;
                    locationAddress = new LocationAddress
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
            }
            catch (Exception ex)
            {
                Api.LogError(ex.Message, ex);
            }

            App.ViewModel.CurrentAddress = locationAddress;
        }

        public async static Task<double> GetDistance()
        {
            try
            {
                if (App.ViewModel.CurrentPosition == null)
                {
                    await GetPosition();
                }

                return new GeoCoordinate(App.ViewModel.CurrentPosition.Latitude, App.ViewModel.CurrentPosition.Longitude)
                    .GetDistanceTo(new GeoCoordinate(App.ViewModel.SelectedLocation.Latitude,
                                                     App.ViewModel.SelectedLocation.Longitude));
            }
            catch (Exception ex)
            {
                Api.LogError(ex.Message, ex);
            }
            return 0;
        }

        public static BitmapImage GetLocationImage()
        {
            return GetImage(App.ViewModel.SelectedLocation.ImageName);
        }

        public static BitmapImage GetLocationImage(string name)
        {
            if (string.IsNullOrEmpty(name)) return new BitmapImage();
            return GetImage(name);
        }

        public static Picture GetPictureByToken(string token)
        {
            var library = new MediaLibrary();
            var photoFromLibrary = library.GetPictureFromToken(token);

            return photoFromLibrary;
        }

        private static BitmapImage GetImage(string name)
        {
            var library = new MediaLibrary();
            var bitmap = new BitmapImage();

            try
            {
                foreach (var picture in library.Pictures.Where(picture => picture.Name == name))
                {
                    bitmap.SetSource(picture.GetImage());
                }
            }
            catch (Exception ex)
            {
                Api.LogError(ex.Message, ex);
            }

            return bitmap;
        }

        public static Position GetPositionFromImage(Stream imageStream)
        {
            var library = new MediaLibrary();
            var position = new Position();

            try
            {
                foreach (var picture in library.Pictures)
                {
                    if (picture.GetImage().Length == imageStream.Length)
                    {
                        var reader = new ExifReader(picture.GetImage());
                        double[] tmplat, tmplong;

                        if (reader.GetTagValue(ExifTags.GPSLatitude, out tmplat) &&
                            reader.GetTagValue(ExifTags.GPSLongitude, out tmplong))
                        {
                            position.Longitude = tmplong[0] + tmplong[1] / 60 +
                                tmplong[2] / (60 * 60);
                            position.Latitude = tmplat[0] + tmplat[1] / 60 +
                                tmplat[2] / (60 * 60);

                            string tmp;
                            if ((reader.GetTagValue(ExifTags.GPSLongitudeRef, out tmp) &&
                                tmp == "W"))
                            {
                                position.Longitude *= -1;
                            }

                            if ((reader.GetTagValue(ExifTags.GPSLatitudeRef, out tmp) &&
                                tmp == "S"))
                            {
                                position.Latitude *= -1;
                            }
                        }
                    }
                }
            }
            catch (ExifLibException ex)
            {
                if (ex.Message.Contains("Could not find Exif data block"))
                {
                    MessageBox.Show(AppResources.NoExifDataMessage, AppResources.NoExifDataMessageTitle, MessageBoxButton.OK);
                }
            }
            catch (Exception ex)
            {
                Api.LogError(ex.Message, ex);
                MessageBox.Show(AppResources.GeneralErrorMessage, AppResources.GeneralErrorMessageTitle, MessageBoxButton.OK);
            }

            return position;
        }

        public static BitmapImage GetThumbnail(string name)
        {
            var library = new MediaLibrary();
            var bitmap = new BitmapImage();

            foreach (var picture in library.Pictures.Where(picture => picture.Name == name))
            {
                bitmap.SetSource(picture.GetThumbnail());
            }

            return bitmap;
        }

        public static void CreateTile()
        {
            try
            {
               var tileData = new RadCycleTileData();
               var dbLIst = App.ViewModel.LoadTileLocations();

                var locationList =
                    dbLIst.Where(
                        x => !String.IsNullOrEmpty(x.ImageUri) && !String.IsNullOrEmpty(x.ImageName))
                        .Reverse()
                        .Take(9)
                        .ToList();

                var uriList = new List<Uri>();
                var i = 0;
                using (var file = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    foreach (var item in locationList)
                    {
                        try
                        {
                            using (var fileStream = file.OpenFile("Shared/ShellContent/" +
                                                              i + ".jpg", FileMode.Create))
                            {
                                var bmp = new WriteableBitmap(GetImage(item.ImageName));
                                bmp.SaveJpeg(fileStream, bmp.PixelWidth, bmp.PixelHeight, 0, 30);

                                uriList.Add(new Uri("isostore:/shared/ShellContent/" + i + ".jpg", UriKind.Absolute));
                                i++;
                            }
                        } catch (Exception) {}
                    }
                }

                tileData.CycleImages = uriList;
                LiveTileHelper.UpdateTile(ShellTile.ActiveTiles.FirstOrDefault(), tileData);
            }
            catch (OutOfMemoryException ex)
            {
                Api.LogError(
                    "Zuviel Speicherverbrauch beim Tile Aktualisieren. Aktueller Memory Verbrauch: " +
                    Microsoft.Phone.Info.DeviceStatus.ApplicationCurrentMemoryUsage + "Peak: " +
                    Microsoft.Phone.Info.DeviceStatus.ApplicationPeakMemoryUsage, ex);
            }
            catch (Exception ex)
            {
                Api.LogError(ex.Message, ex);
            } 
        }

        public static string GetImageName(Stream choosenStream)
        {
            var library = new MediaLibrary();

            foreach (var image in from image in library.Pictures let stream = image.GetImage() where stream.Length == choosenStream.Length select image)
            {
                return image.Name;
            }

            return String.Empty;
        }
    }
}
