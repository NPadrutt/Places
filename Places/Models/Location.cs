using System;
using System.ComponentModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Windows.Media.Imaging;
using FlurryWP8SDK;
using Places.Src;

namespace Places.Models
{
    [Table]
    public class Location : INotifyPropertyChanged, IComparable
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

        private string _imageName;

        [Column]
        public string ImageName
        {
            get { return _imageName; }
            set
            {
                if (_imageName != value)
                {
                    _imageName = value;
                    NotifyPropertyChanged("LocationImageName");
                }
            }
        }

        private string _imageUri;

        [Column]
        public string ImageUri
        {
            get { return _imageUri; }
            set
            {
                if (_imageUri != value)
                {
                    _imageUri = value;
                    NotifyPropertyChanged("ImageUri");
                }
            }
        }

        public BitmapImage LocationImage
        {
            get { return Utilities.GetLocationImage(_imageName); }
        }

        private BitmapImage _thumbnail;
        public BitmapImage Thumbnail
        {
            get { return _thumbnail; }
            set
            {
                if (_thumbnail != value)
                {
                    _thumbnail = value;
                    NotifyPropertyChanged("Thumbnail");
                }
            }
        }

        private string _name;

        [Column]
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }

        private double _latitude;

        [Column]
        public double Latitude
        {
            get { return _latitude; }
            set
            {
                if (_latitude != value)
                {
                    _latitude = value;
                    NotifyPropertyChanged("Latitude");
                }
            }
        }

        private double _longitude;

        [Column]
        public double Longitude
        {
            get { return _longitude; }
            set
            {
                if (_longitude != value)
                {
                    _longitude = value;
                    NotifyPropertyChanged("Longitude");
                }
            }
        }

        private double _accuracy;

        [Column]
        public double Accuracy
        {
            get { return _accuracy; }
            set
            {
                if (_accuracy != value)
                {
                    _accuracy = value;
                    NotifyPropertyChanged("Accuracy");
                }
            }
        }

        private double? _distance;

        [Column]
        public double? Distance
        {
            get { return _distance; }
            set
            {
                if (_distance != value)
                {
                    _distance = value;
                    NotifyPropertyChanged("Distance");
                }
            }
        }

        private string _comment;

        [Column]
        public string Comment
        {
            get { return _comment; }
            set
            {
                if (_comment != value)
                {
                    _comment = value;
                    NotifyPropertyChanged("Comment");
                }
            }
        }

        #region Foreign Keys
        
        [Column]
        private int? _addressId;

        private EntityRef<LocationAddress> _locationAddress;

        [Association(Storage = "_locationAddress", ThisKey = "_addressId", OtherKey = "Id", IsForeignKey = true, DeleteRule = "Cascade")]
        public LocationAddress LocationAddress
        {
            get { return _locationAddress.Entity; }
            set
            {
                NotifyPropertyChanging("LocationAddress");
                _locationAddress.Entity = value;

                if (value != null)
                {
                    _addressId = value.Id;
                }

                NotifyPropertyChanging("LocationAddress");
            }
        }

        private EntitySet<Tag> _tag;

        [Association(Storage = "_tag", OtherKey = "_locationId", ThisKey = "Id")]
        public EntitySet<Tag> Tags
        {
            get { return _tag; }
            set { _tag.Assign(value); }
        }
 
        public Location()
        {
            _tag = new EntitySet<Tag>(
                tag => tag.Location = this,
                tag => tag.Location = null);
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

        public int CompareTo(object obj)
        {
            var location = obj as Location;
            return location != null ? location.Name.CompareTo(Name) : 0;
        }
    }
}
