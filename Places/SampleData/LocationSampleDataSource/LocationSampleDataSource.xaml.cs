﻿//      *********    DO NOT MODIFY THIS FILE     *********
//      This file is regenerated by a design tool. Making
//      changes to this file can cause errors.

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace Expression.Blend.SampleData.LocationSampleDataSource
{
    using System;

    // To significantly reduce the sample data footprint in your production application, you can set
    // the DISABLE_SAMPLE_DATA conditional compilation constant and disable sample data at runtime.
#if DISABLE_SAMPLE_DATA
	internal class LocationSampleDataSource { }
#else

    public class LocationSampleDataSource : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public LocationSampleDataSource()
        {
            try
            {
                Uri resourceUri = new Uri("/Places;component/SampleData/LocationSampleDataSource/LocationSampleDataSource.xaml", UriKind.Relative);
                if (Application.GetResourceStream(resourceUri) != null)
                {
                    Application.LoadComponent(this, resourceUri);
                }
            }
            catch (Exception)
            {
            }
        }

        private AllLocations _AllLocations = new AllLocations();

        public AllLocations AllLocations
        {
            get
            {
                return _AllLocations;
            }
        }
    }

    public class AllLocations : ObservableCollection<AllLocationsItem>
    {
    }

    public class AllLocationsItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private string _Name = string.Empty;

        public string Name
        {
            get
            {
                return _Name;
            }

            set
            {
                if (_Name != value)
                {
                    _Name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        private string _Latitude = string.Empty;

        public string Latitude
        {
            get
            {
                return _Latitude;
            }

            set
            {
                if (_Latitude != value)
                {
                    _Latitude = value;
                    OnPropertyChanged("Latitude");
                }
            }
        }

        private string _Longtitude = string.Empty;

        public string Longtitude
        {
            get
            {
                return _Longtitude;
            }

            set
            {
                if (_Longtitude != value)
                {
                    _Longtitude = value;
                    OnPropertyChanged("Longtitude");
                }
            }
        }

        private Address _Address = new Address();

        public Address Address
        {
            get
            {
                return _Address;
            }
        }
    }

    public class AddressItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private string _Street = string.Empty;

        public string Street
        {
            get
            {
                return _Street;
            }

            set
            {
                if (_Street != value)
                {
                    _Street = value;
                    OnPropertyChanged("Street");
                }
            }
        }

        private double _HouseNumber = 0;

        public double HouseNumber
        {
            get
            {
                return _HouseNumber;
            }

            set
            {
                if (_HouseNumber != value)
                {
                    _HouseNumber = value;
                    OnPropertyChanged("HouseNumber");
                }
            }
        }

        private double _PostalCode = 0;

        public double PostalCode
        {
            get
            {
                return _PostalCode;
            }

            set
            {
                if (_PostalCode != value)
                {
                    _PostalCode = value;
                    OnPropertyChanged("PostalCode");
                }
            }
        }

        private string _City = string.Empty;

        public string City
        {
            get
            {
                return _City;
            }

            set
            {
                if (_City != value)
                {
                    _City = value;
                    OnPropertyChanged("City");
                }
            }
        }

        private string _District = string.Empty;

        public string District
        {
            get
            {
                return _District;
            }

            set
            {
                if (_District != value)
                {
                    _District = value;
                    OnPropertyChanged("District");
                }
            }
        }

        private string _State = string.Empty;

        public string State
        {
            get
            {
                return _State;
            }

            set
            {
                if (_State != value)
                {
                    _State = value;
                    OnPropertyChanged("State");
                }
            }
        }
    }

    public class Address : ObservableCollection<AddressItem>
    {
    }

#endif
}