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
using Microsoft.Xna.Framework.Media;

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
            }
            catch (Exception ex)
            {
                Api.LogError(ex.Message, ex.InnerException);
            }

            App.ViewModel.CurrentAddress = locationAddress;
        }        

        public static BitmapImage LoadLocationImage()
        {
            return GetImage(App.ViewModel.SelectedLocation.ImageName);
        }

        public static BitmapImage LoadLocationImage(string name)
        {
            if (string.IsNullOrEmpty(name)) return new BitmapImage();
            return GetImage(name);
        }        

        private static BitmapImage GetImage(string name)
        {
            var library = new MediaLibrary();
            var bitmap = new BitmapImage();

            foreach (var picture in library.Pictures)
            {
                if (picture.Name == name)
                {
                    bitmap.SetSource(picture.GetImage());
                }
            }

            return bitmap;
        }

        public static BitmapImage GetThumbnail(string name)
        {
            var library = new MediaLibrary();
            var bitmap = new BitmapImage();

            foreach (var picture in library.Pictures)
            {
                if (picture.Name == name)
                {
                    bitmap.SetSource(picture.GetThumbnail()); 
                }
            }

            return bitmap;
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
            try
            {
                var tileData = new RadCycleTileData();

                List<Location> locationList = App.ViewModel.AllLocations.Where(x => !String.IsNullOrEmpty(x.ImageUri) && !String.IsNullOrEmpty(x.ImageName))
                    .Take(9)
                    .ToList();

                var fotolist = new List<BitmapImage>();
                foreach (var item in locationList)
                {
                    fotolist.Add(GetImage(item.ImageName));
                }

                var uriList = new List<Uri>();
                int i = 0;

                foreach (var item in fotolist)
                {
                    using (IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        using (IsolatedStorageFileStream stream = file.OpenFile("Shared/ShellContent/" +
                          i + ".jpg", FileMode.Create))
                        {
                            WriteableBitmap bmp = new WriteableBitmap((BitmapSource)item);
                            bmp.SaveJpeg(stream, bmp.PixelWidth, bmp.PixelHeight, 0, 100);
                        }
                    }
                    uriList.Add(new Uri("isostore:/shared/ShellContent/" + i + ".jpg", UriKind.Absolute));
                    i++;
                }

                tileData.CycleImages = uriList;
                LiveTileHelper.UpdateTile(ShellTile.ActiveTiles.FirstOrDefault(), tileData);
            }
            catch (Exception ex)
            {
                Api.LogError(ex.Message, ex.InnerException);                
            }            
        }

        public static string GetImageName(Stream choosenStream)
        {
            var library = new MediaLibrary();

            foreach (var image in library.Pictures)
            {
                Stream stream = image.GetImage();
                if (stream.Length == choosenStream.Length)
                {
                    return image.Name;
                }
            }

            return String.Empty;
        }
    }
}
