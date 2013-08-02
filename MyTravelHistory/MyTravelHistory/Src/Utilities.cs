﻿using System;
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
using ExifLib;

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

        public static Position GetPositionFromImage(Stream imageStream)
        {
            var library = new MediaLibrary();
            var position = new Position();

            foreach (var picture in library.Pictures)
            {
                if (picture.GetImage().Length == imageStream.Length)
                {
                    ExifReader reader = new ExifReader(picture.GetImage());
                    double[] tmplat, tmplong;

                    if (reader.GetTagValue<double[]>(ExifTags.GPSLatitude, out tmplat) &&
                        reader.GetTagValue<double[]>(ExifTags.GPSLongitude, out tmplong))
                    {
                        position.Longitude = tmplong[0] + tmplong[1] / 60 +
                            tmplong[2] / (60 * 60);
                        position.Latitude = tmplat[0] + tmplat[1] / 60 +
                            tmplat[2] / (60 * 60);

                        string tmp;
                        if ((reader.GetTagValue<string>(ExifTags.GPSLongitudeRef, out tmp) &&
                            tmp == "W"))
                        {
                            position.Longitude *= -1;
                        }

                        if ((reader.GetTagValue<string>(ExifTags.GPSLatitudeRef, out tmp) &&
                            tmp == "S"))
                        {
                            position.Latitude *= -1;
                        }
                    }
                }
            }

            return position;
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
                            bmp.SaveJpeg(stream, bmp.PixelWidth, bmp.PixelHeight, 0, 60);
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
