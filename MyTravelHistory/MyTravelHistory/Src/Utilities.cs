using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using FlurryWP8SDK;
using Windows.Devices.Geolocation;
using Microsoft.Phone.Maps.Services;
using System.Device.Location;
using Microsoft.Phone.Shell;
using MyTravelHistory.Models;
using Telerik.Windows.Controls;
using System.IO.IsolatedStorage;

namespace MyTravelHistory.Src
{
    public class Utilities
    {
        private const string ImageFolder = "//Shared//ShellContent";

        public static string GetVersion()
        {
            return Assembly.GetExecutingAssembly().FullName.Split('=')[1].Split(',')[0];
        }

        public static async Task GetPosition()
        {
            var geolocater = new Geolocator { DesiredAccuracy = PositionAccuracy.High };

            try
            {
                var geoposition = await geolocater.GetGeopositionAsync(
                    maximumAge: TimeSpan.FromMinutes(2),
                    timeout: TimeSpan.FromSeconds(20)
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
                Api.LogError("timeout", ex.InnerException);
            }
            catch (Exception ex)
            {
                if ((uint)ex.HResult == 0x80004004)
                {
                    // the application does not have the right capability or the location master switch is off
                    Api.LogError("location  is disabled in phone settings.", ex.InnerException);
                }
                if ((uint)ex.HResult == 0x800705B4)
                {
                    // the application does not have the right capability or the location master switch is off
                    Api.LogError("Timeout expired.", ex.InnerException);
                }
            }
        }

        public static async Task GetAddress(double latitude, double longtitude)
        {
            try
            {
                var myReverseGeocodeQuery = new ReverseGeocodeQuery
                                {
                                    GeoCoordinate = new GeoCoordinate(latitude, longtitude)
                                };
                IList<MapLocation> locations = await myReverseGeocodeQuery.GetMapLocationsAsync();

                App.ViewModel.CurrentAddress = locations.FirstOrDefault().Information.Address;
            }
            catch (Exception ex)
            {
                if ((uint) ex.HResult == 0x80041B56)
                {
                    Api.LogError("Task Unsuccesfull", ex.InnerException);
                }
            }
        }

        public static string SaveImageToLocalStorage(BitmapImage image)
        {
            string imagename = Guid.NewGuid() + ".jpg";

            SaveImageToLocalStorage(image, imagename);

            return imagename;
        }

        public static void SaveImageToLocalStorage(BitmapImage image, string imagename)
        {
            try
            {
                using (var myIsoStorage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (!myIsoStorage.DirectoryExists(ImageFolder))
                    {
                        myIsoStorage.CreateDirectory(ImageFolder);
                    }

                    if (myIsoStorage.FileExists(imagename))
                    {
                        myIsoStorage.DeleteFile(imagename);
                    }

                    string path = Path.Combine(ImageFolder, imagename);
                    using (var stream = myIsoStorage.CreateFile(path))
                    {
                        var wb = new WriteableBitmap(image);
                        wb.SaveJpeg(stream, image.PixelWidth, image.PixelHeight, 0, 50);
                    }
                }
            }
            catch (Exception ex)
            {
                Api.LogError(ex.Message, ex.InnerException);
            }
        }

        public static BitmapImage LoadLocationImage()
        {
            return LoadImage(App.ViewModel.SelectedLocation.LocationImageName);
        }

        public static BitmapImage LoadLocationImage(string name)
        {
            if (string.IsNullOrEmpty(name)) return new BitmapImage();
            return LoadImage(name);
        }

        private static BitmapImage LoadImage(string imagename)
        {
            var bmp = new BitmapImage();

            try
            {
                using (var myIsoStorage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (!myIsoStorage.DirectoryExists(ImageFolder))
                    {
                        return bmp;
                    }

                    string path = Path.Combine(ImageFolder, imagename);
                    if (myIsoStorage.FileExists(path))
                    {
                        using (var imageStream = myIsoStorage.OpenFile(path, FileMode.Open, FileAccess.Read))
                        {
                            bmp.SetSource(imageStream);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Api.LogError(ex.Message, ex.InnerException);
            }

            return bmp;
        }

        public static Stream GetImageStream(string imagename)
        {
            var stream = Stream.Null;

            try
            {
                var myIsoStorage = IsolatedStorageFile.GetUserStoreForApplication();
                
                    if (!myIsoStorage.DirectoryExists(ImageFolder))
                    {
                        return stream;
                    }

                    string path = Path.Combine(ImageFolder, imagename);
                    if (myIsoStorage.FileExists(path))
                    {
                        using (var imageStream = myIsoStorage.OpenFile(path, FileMode.Open, FileAccess.Read))
                        {
                            stream = imageStream;
                        }
                    }
                
            }
            catch (Exception ex)
            {
                Api.LogError(ex.Message, ex.InnerException);
            }

            return stream;
        }

        public static void DeleteImage(string imagename)
        {
            using (var myIsoStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!myIsoStorage.DirectoryExists(ImageFolder))
                {
                    myIsoStorage.CreateDirectory(ImageFolder);
                }

                string path = Path.Combine(ImageFolder, imagename);
                if (myIsoStorage.FileExists(path))
                {
                    myIsoStorage.DeleteFile(path);
                }
            }
        }

        public static Uri GetImageUri(string imagename)
        {
            string path = Path.Combine(ImageFolder, imagename);

            return new Uri(@"isostore:/" + path, UriKind.Absolute);
        }

        public static void CreateTile()
        {
            var tileData = new RadCycleTileData();

            List<string> stringList = App.ViewModel.AllLocations.Select(x => x.LocationImageName != null ? x.LocationImageName : String.Empty)
                .Take(9)
                .ToList();

            var uriList = (from uriString in stringList where uriString != String.Empty select GetImageUri(uriString)).ToList();

            tileData.CycleImages = uriList;
            LiveTileHelper.UpdateTile(ShellTile.ActiveTiles.FirstOrDefault(), tileData);
            
        }
    }
}
