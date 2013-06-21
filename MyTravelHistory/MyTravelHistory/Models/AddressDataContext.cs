using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTravelHistory.Models
{
    [Table]
    public class Address : INotifyPropertyChanged, INotifyPropertyChanging
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

        private string _street;

        [Column]
        public string Street
        {
            get { return _street; }
            set
            {
                if (_street != value)
                {
                    NotifyPropertyChanging("Street");
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
                    NotifyPropertyChanging("HouseNumber");
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
                    NotifyPropertyChanging("PostalCode");
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
                    NotifyPropertyChanging("City");
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
                    NotifyPropertyChanging("District");
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
                    NotifyPropertyChanging("State");
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
                    NotifyPropertyChanging("Country");
                    _country = value;
                    NotifyPropertyChanged("Country");
                }
            }
        }

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
