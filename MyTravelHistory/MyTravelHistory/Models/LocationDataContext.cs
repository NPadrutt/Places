using MyTravelHistory.Src;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MyTravelHistory.Models
{
    [Table]
    public class Location : INotifyPropertyChanged, INotifyPropertyChanging
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
                    NotifyPropertyChanging("Id");
                    _id = value;
                    NotifyPropertyChanged("Id");
                }
            }
        }

        private byte[] _locationImage;

        [Column(DbType = "image")]
        public byte[] LocationImage
        {
            get { return _locationImage; }
            set
            {
                if (_locationImage != value)
                {
                    NotifyPropertyChanging("LocationImage");
                    _locationImage = value;
                    NotifyPropertyChanged("LocationImage");
                }
            }
        }

        public WriteableBitmap Thumbnail
        {
            get
            {
                if (_locationImage != null)
                {
                    return Utilities.ConvertToImage(_locationImage, 100, 200);
                }
                
                return new WriteableBitmap(0,0);
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
                    NotifyPropertyChanging("Name");
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
                    NotifyPropertyChanging("Latitude");
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
                    NotifyPropertyChanging("Longitude");
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
                    NotifyPropertyChanging("Accuracy");
                    _accuracy = value;
                    NotifyPropertyChanged("Accuracy");
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
                    NotifyPropertyChanging("Comment");
                    _comment = value;
                    NotifyPropertyChanged("Comment");
                }
            }
        }

        #region Foreign Keys
        
        [Column]
        private int? _addressId;

        private EntityRef<LocationAddress> _locationAddress;

        [Association(Storage = "_locationAddress", ThisKey = "_addressId", OtherKey = "Id", IsForeignKey = true)]
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

        #region INotifyPropertyChanging Members

        public event PropertyChangingEventHandler PropertyChanging;

        // Used to notify that a property is about to change
        private void NotifyPropertyChanging(string propertyName)
        {
            if (PropertyChanging != null)
            {
                PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
            }
        }

        #endregion
    }
}
