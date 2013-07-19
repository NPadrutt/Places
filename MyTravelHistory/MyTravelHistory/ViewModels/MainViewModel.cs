using Microsoft.Phone.Data.Linq;
using Microsoft.Phone.Maps.Services;
using MyTravelHistory.Models;
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
            }
            else
            {
                var updateHelper = new UpdateHelper();
                updateHelper.UpdateDatabase(db);
            }
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

        private ObservableCollection<Location> _selectedLocations;
        public ObservableCollection<Location> SelectedLocations
        {
            get { return _selectedLocations; }
            set
            {
                _selectedLocations = value;
                NotifyPropertyChanged("SelectedLocations");
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
            var locationItemsInDb = from Location location in db.Locations
                                    join LocationAddress adr in db.LocationAddresses
                                    on location.LocationAddress.Id equals adr.Id
                                    orderby location.Name
                                    select location;

            AllLocations = new ObservableCollection<Location>(locationItemsInDb);
            SelectedLocations = AllLocations;
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

        public void DeleteLocation(Tag TagToDelete)
        {
            AllTags.Remove(TagToDelete);
            db.Tags.DeleteOnSubmit(v);

            db.SubmitChanges();
        }

        public void LoadTags()
        {
            var TagsItemsInDb = from Tag tag in db.Tags
                                    orderby tag.TagName
                                    select tag;

           AllTags = new ObservableCollection<Location>(TagsItemsInDb);
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
