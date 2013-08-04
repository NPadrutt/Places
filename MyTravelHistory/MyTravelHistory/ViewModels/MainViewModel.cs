using Microsoft.Phone.Data.Linq;
using Microsoft.Phone.Maps.Services;
using MyTravelHistory.Models;
using MyTravelHistory.Resources;
using MyTravelHistory.Src;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace MyTravelHistory.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private MainDataContext db;

        public MainViewModel()
        {
            db = new MainDataContext(MainDataContext.DBConnectionString);
            CreateDbIfNotExist();
        }

        private void CreateDbIfNotExist()
        {
            DatabaseSchemaUpdater schemaUpdate;

            // Create the database if it does not exist.
            if (!db.DatabaseExists())
            {
                db.CreateDatabase();
                schemaUpdate = db.CreateDatabaseSchemaUpdater();
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

            AddTag(tagBar);
            AddTag(tagRestaurant);
            AddTag(tagHotel);
            AddTag(tagShop);
            AddTag(tagWork);
            AddTag(tagMuseum);
            AddTag(tagViewpoint);
        }

        public void SaveChangesToDB()
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

        #region Location

        private Location _selectedLocation;
        public Location SelectedLocation
        {
            get { return _selectedLocation; }
            set
            {
                _selectedLocation = value;
                NotifyPropertyChanged("SelectedLocation");
            }
        }

        private ObservableCollection<Location> _allLocations;
        public ObservableCollection<Location> AllLocations
        {
            get { return _allLocations; }
            set
            {
                _allLocations = value;
                NotifyPropertyChanged("AllLocations");
            }
        }

        public void AddLocation(Location newLocation)
        {
            AllLocations.Add(newLocation);
            db.Locations.InsertOnSubmit(newLocation);

            db.SubmitChanges();
        }

        public void DeleteLocation(Location LocationToDelete)
        {
            AllLocations.Remove(LocationToDelete);
            db.Locations.DeleteOnSubmit(LocationToDelete);

            db.SubmitChanges();
        }

        public void LoadLocations()
        {
            var locationItemsInDb = from location in db.Locations
                                    join LocationAddress adr in db.LocationAddresses
                                        on new { location.LocationAddress.Id } equals new { adr.Id }
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
            AllTags.Remove(TagToDelete);
            db.Tags.DeleteOnSubmit(TagToDelete);

            db.SubmitChanges();
        }

        public void LoadTags()
        {
            var TagsItemsInDb = from Tag tag in db.Tags
                                    orderby tag.TagName
                                    select tag;

           AllTags = new ObservableCollection<Tag>(TagsItemsInDb);
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
