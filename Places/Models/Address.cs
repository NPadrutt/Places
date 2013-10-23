using System.ComponentModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace Places.Models
{
    [Table]
    public class LocationAddress : INotifyPropertyChanged
    {
        private int _id;

        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL Identity", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public int Id
        {
            get { return _id; }
            set
            {
                if (_id != value)
                {
                    _id = value;
                    NotifyPropertyChanged("Id");
                }
            }
        }

        private string _street;

        [Column]
        public string Street
        {
            get { return _street; }
            set
            {
                if (_street != value)
                {
                    _street = value;
                    NotifyPropertyChanged("Street");
                }
            }
        }

        private string _houseNumber;

        [Column]
        public string HouseNumber
        {
            get { return _houseNumber; }
            set
            {
                if (_houseNumber != value)
                {
                    _houseNumber = value;
                    NotifyPropertyChanged("HouseNumber");
                }
            }
        }

        private string _postalCode;

        [Column]
        public string PostalCode
        {
            get { return _postalCode; }
            set
            {
                if (_postalCode != value)
                {
                    _postalCode = value;
                    NotifyPropertyChanged("PostalCode");
                }
            }
        }

        private string _city;

        [Column]
        public string City
        {
            get { return _city; }
            set
            {
                if (_city != value)
                {
                    _city = value;
                    NotifyPropertyChanged("City");
                }
            }
        }

        private string _district;

        [Column]
        public string District
        {
            get { return _district; }
            set
            {
                if (_district != value)
                {
                    _district = value;
                    NotifyPropertyChanged("District");
                }
            }
        }

        private string _state;

        [Column]
        public string State
        {
            get { return _state; }
            set
            {
                if (_state != value)
                {
                    _state = value;
                    NotifyPropertyChanged("State");
                }
            }
        }

        private string _country;

        [Column]
        public string Country
        {
            get { return _country; }
            set
            {
                if (_country != value)
                {
                    _country = value;
                    NotifyPropertyChanged("Country");
                }
            }
        }

        #region Foreign Keys

        private EntitySet<Location> _location;

        [Association(Storage = "_location", OtherKey = "_addressId", ThisKey = "Id")]
        public EntitySet<Location> Location
        {
            get { return _location; }
            set { _location.Assign(value); }
        }

        public LocationAddress()
        {
            _location = new EntitySet<Location>(
                attach_Location,
                detach_Location
                );
        }

        private void attach_Location(Location location)
        {
            NotifyPropertyChanging("Location");
            location.LocationAddress = this;
        }

        private void detach_Location(Location location)
        {
            NotifyPropertyChanging("Location");
            location.LocationAddress = null;
        }

        #endregion

        [Column(IsVersion = true)]
        private Binary _version;


        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify that a property changed
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
