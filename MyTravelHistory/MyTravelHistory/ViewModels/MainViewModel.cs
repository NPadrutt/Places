using Microsoft.Phone.Data.Linq;
using MyTravelHistory.Models;
using MyTravelHistory.Src;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                UpdateHelper updateHelper = new UpdateHelper();
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
                                    orderby location.Name
                                    select location;

            AllLocations = new ObservableCollection<Location>(locationItemsInDb);
        }

        #endregion

        #region Position

        private Position _selectedPosition;
        public Position SelectedPosition
        {
            get { return _selectedPosition; }
            set
            {
                _selectedPosition = value;
                NotifyPropertyChanged("SelectedPosition");
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
