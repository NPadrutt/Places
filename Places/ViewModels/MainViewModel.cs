﻿using BugSense;
using Microsoft.Phone.Data.Linq;
using Places.Models;
using Places.Resources;
using Places.Src;
using PropertyChanged;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using StringComparer = Places.Src.StringComparer;

namespace Places.ViewModels
{
    [ImplementPropertyChanged]
    public class MainViewModel
    {
        private readonly MainDataContext db;

        public MainViewModel()
        {
            db = new MainDataContext(MainDataContext.DBConnectionString);
            CreateDbIfNotExist();
        }

        private void CreateDbIfNotExist()
        {
            if (!db.DatabaseExists())
            {
                db.CreateDatabase();
                var schemaUpdate = db.CreateDatabaseSchemaUpdater();
                schemaUpdate.DatabaseSchemaVersion = db.SCHEMAVERSION;
                schemaUpdate.Execute();

                LoadTags();
                CreateDefaultEntries();
            }
            else
            {
                var updateHelper = new UpdateHelper();
                updateHelper.UpdateDatabase(db);
            }
        }

        private void CreateDefaultEntries()
        {
            var tagBar = new Tag { TagName = AppResources.BarLabel };
            var tagRestaurant = new Tag { TagName = AppResources.RestaurantLabel };
            var tagHotel = new Tag { TagName = AppResources.HotelLabel };
            var tagShop = new Tag { TagName = AppResources.ShopLabel };
            var tagWork = new Tag { TagName = AppResources.WorkLabel };
            var tagMuseum = new Tag { TagName = AppResources.MuseumLabel };
            var tagViewpoint = new Tag { TagName = AppResources.ViewpointLabel };
            var tagFriends = new Tag { TagName = AppResources.FriendsLabel };
            var tagFamily = new Tag { TagName = AppResources.FamilyLabel };
            var tagClub = new Tag { TagName = AppResources.ClubLabel };
            var tagTouristAttraction = new Tag { TagName = AppResources.TouristAttractionLabel };

            AddTag(tagBar);
            AddTag(tagRestaurant);
            AddTag(tagHotel);
            AddTag(tagShop);
            AddTag(tagWork);
            AddTag(tagMuseum);
            AddTag(tagViewpoint);
            AddTag(tagFriends);
            AddTag(tagFamily);
            AddTag(tagClub);
            AddTag(tagTouristAttraction);
        }

        public void SaveChangesToDb()
        {
            db.SubmitChanges();
        }

        public void DeleteDatabase()
        {
            if (db.DatabaseExists())
            {
                db.DeleteDatabase();
                db.Dispose();
            }
        }

        public ObservableCollection<string> AllCities { get; set; }

        public void LoadCities()
        {
            var addressItemsInDb = from address in db.LocationAddresses
                                   orderby address.City
                                   select address.City;

            AllCities = new ObservableCollection<string>();
            new ObservableCollection<string>(addressItemsInDb).Distinct(new StringComparer()).ToList().ForEach(x => AllCities.Add(x));
            AllCities.Insert(0, AppResources.AllLabel);
        }

        private Location selectedLocation;

        public Location SelectedLocation { get; set; }

        public Stream SelectedImageStream { get; set; }

        public string SelectedCity { get; set; }

        public ObservableCollection<Location> AllLocations { get; set; }

        public int LocationCount { get { return db.Locations.Count(); } }

        public void AddLocation(Location newLocation)
        {
            if (AllLocations == null)
            {
                LoadLocationsByCity(newLocation.LocationAddress.City);
            }
            if (AllCities != null && newLocation.LocationAddress != null
                && newLocation.LocationAddress.City != null)
            {
                AllCities.Add(newLocation.LocationAddress.City);
            }

            AllLocations.Add(newLocation);
            var unsortedLocations = AllLocations;
            AllLocations = new ObservableCollection<Location>(
                    unsortedLocations.OrderByDescending(location => location)
                    );

            var unsortedLocation = AllLocations;
            AllLocations = new ObservableCollection<Location>(
                unsortedLocation.OrderByDescending(sleep => sleep)
                );

            db.Locations.InsertOnSubmit(newLocation);
            db.SubmitChanges();
        }

        public void DeleteLocation(Location LocationToDelete)
        {
            if (LocationToDelete.Tags != null)
            {
                LocationToDelete.Tags = null;
            }
            AllLocations.Remove(LocationToDelete);
            db.Locations.DeleteOnSubmit(LocationToDelete);
            db.LocationAddresses.DeleteOnSubmit(LocationToDelete.LocationAddress);

            db.SubmitChanges();
        }

        public void LoadLocations()
        {
            var locationItemsInDb = from location in db.Locations
                                    join LocationAddress adr in db.LocationAddresses
                                        on new { location.LocationAddress.Id } equals new { adr.Id }
                                    orderby location.Name
                                    select location;

            AllLocations = new ObservableCollection<Location>(locationItemsInDb);

            foreach (var location in AllLocations)
            {
                if (!string.IsNullOrEmpty(location.ImageName))
                {
                    location.Thumbnail = Utilities.GetThumbnail(location.ImageName);
                }
            }
        }

        public void LoadLocationsByCity(string city)
        {
            var locationItemsInDb = from location in db.Locations
                                    join LocationAddress adr in db.LocationAddresses
                                        on new { location.LocationAddress.Id } equals new { adr.Id }
                                    where location.LocationAddress.City == city
                                    orderby location.LocationAddress.City
                                    select location;

            AllLocations = new ObservableCollection<Location>(locationItemsInDb);

            foreach (var location in AllLocations.Where(location => !string.IsNullOrEmpty(location.ImageName)))
            {
                location.Thumbnail = Utilities.GetThumbnail(location.ImageName);
            }
        }

        public Location LoadPinnedLocation(int id)
        {
            try
            {
                var locationItemsInDb = from location in db.Locations
                                        join LocationAddress adr in db.LocationAddresses
                                            on new { location.LocationAddress.Id } equals new { adr.Id }
                                        where location.Id == id
                                        orderby location.Name
                                        select location;

                return new ObservableCollection<Location>(locationItemsInDb).First();
            }
            catch (Exception ex)
            {
                BugSenseHandler.Instance.LogException(ex);
                MessageBox.Show(AppResources.LocationNoExistingMessage, AppResources.LocationNotExistingTitle,
                    MessageBoxButton.OK);
            }

            return new Location();
        }

        public ObservableCollection<Location> LoadTileLocations()
        {
            var skipCount = new Random().Next(0, db.Locations.Count() - 4);

            var locationItemsInDb = (from location in db.Locations
                                     where location.ImageName != null && location.ImageUri != null
                                     select location).Skip(skipCount).Take(4);

            return new ObservableCollection<Location>(locationItemsInDb);
        }

        public ObservableCollection<Tag> AllTags { get; set; }

        public void AddTag(Tag newTag)
        {
            AllTags.Add(newTag);
            db.Tags.InsertOnSubmit(newTag);

            db.SubmitChanges();
        }

        public void DeleteTag(Tag TagToDelete)
        {
            foreach (var location in AllLocations)
            {
                if (location.Tags.Contains(TagToDelete))
                {
                    MessageBox.Show(string.Format(AppResources.TagAssignedMessage, location.Name), AppResources.TagAssignedTitle, MessageBoxButton.OK);
                    return;
                }
            }

            AllTags.Remove(TagToDelete);
            db.Tags.DeleteOnSubmit(TagToDelete);

            db.SubmitChanges();
        }

        public void LoadTags()
        {
            var tagsItemsInDb = from Tag tag in db.Tags
                                orderby tag.TagName
                                select tag;

            AllTags = new ObservableCollection<Tag>(tagsItemsInDb);
        }

        public Position CurrentPosition { get; set; }

        public LocationAddress CurrentAddress { get; set; }
    }
}