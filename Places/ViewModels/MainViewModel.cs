﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Phone.Data.Linq;
using Places.Models;
using Places.Resources;
using Places.Src;

namespace Places.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly MainDataContext db;

        public MainViewModel()
        {
            db = new MainDataContext(MainDataContext.DBConnectionString);
            CreateDbIfNotExist();
        }

        private void CreateDbIfNotExist()
        {
            // Create the database if it does not exist.
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

        #region Address

        private ObservableCollection<string> allCities;
        public ObservableCollection<string> AllCities
        {
            get { return allCities; }
            set
            {
                allCities = value;
                NotifyPropertyChanged("AllCities");
            }
        }

        public void LoadCities()
        {
            var addressItemsInDb = from address in db.LocationAddresses
                                   orderby address.City
                                   select address.City;

            AllCities = new ObservableCollection<string>();
            new ObservableCollection<string>(addressItemsInDb).Distinct(new StringComparer()).ToList().ForEach(x => AllCities.Add(x));
            AllCities.Insert(0, AppResources.AllLabel);
        }

        #endregion

        #region Location

        private Location selectedLocation;
        public Location SelectedLocation
        {
            get { return selectedLocation; }
            set
            {
                selectedLocation = value;
                NotifyPropertyChanged("SelectedLocation");
            }
        }

        private Stream selectedImageStream;
        public Stream SelectedImageStream
        {
            get { return selectedImageStream; }
            set
            {
                selectedImageStream = value;
                NotifyPropertyChanged("SelectedImageStream");
            }
        }

        private string selectedCity;
        public string SelectedCity
        {
            get { return selectedCity; }
            set
            {
                selectedCity = value;
                NotifyPropertyChanged("SelectedCity");
            }
        }

        private ObservableCollection<Location> allLocations;
        public ObservableCollection<Location> AllLocations
        {
            get { return allLocations; }
            set
            {
                allLocations = value;
                NotifyPropertyChanged("AllLocations");
            }
        }

        public void AddLocation(Location newLocation)
        {
            if (AllLocations == null)
            {
                LoadLocationsByCity(newLocation.LocationAddress.City);
            }
            if (AllCities != null)
            {
                AllCities.Add(newLocation.LocationAddress.City);
            }
            AllLocations.Add(newLocation);

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
                    on new {location.LocationAddress.Id} equals new {adr.Id}
                orderby location.LocationAddress.City
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
                    on new {location.LocationAddress.Id} equals new {adr.Id}
                where location.LocationAddress.City == city
                orderby location.LocationAddress.City
                select location;

            AllLocations = new ObservableCollection<Location>(locationItemsInDb);

            foreach (var location in AllLocations.Where(location => !string.IsNullOrEmpty(location.ImageName)))
            {
                location.Thumbnail = Utilities.GetThumbnail(location.ImageName);
            }
        }

        public void LoadTileLocations()
        {
            var locationItemsInDb = (from location in db.Locations
                where location.LocationImage != null
                orderby location.LocationAddress.City
                select location).Take(9);

            AllLocations = new ObservableCollection<Location>(locationItemsInDb);
        }

        #endregion

        #region Tags

        private ObservableCollection<Tag> _allTags;
        public ObservableCollection<Tag> AllTags
        {
            get { return _allTags; }
            set
            {
                _allTags = value;
                NotifyPropertyChanged("AllTags");
            }
        }

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

        #endregion

        #region Position

        private Position _currentPosition;
        public Position CurrentPosition
        {
            get { return _currentPosition; }
            set
            {
                _currentPosition = value;
                NotifyPropertyChanged("CurrentPosition");
            }
        }

        private LocationAddress _currentAddress;
        public LocationAddress CurrentAddress
        {
            get { return _currentAddress; }
            set
            {
                _currentAddress = value;
                NotifyPropertyChanged("CurrentAddress");
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify Silverlight that a property has changed.
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
