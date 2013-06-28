using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTravelHistory.Models
{
    public class Position : INotifyPropertyChanged
    {
        private double _latitude;
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

        private DateTime _timeStamp;
        public DateTime Timestamp
        {
            get { return _timeStamp; }
            set
            {
                if (_timeStamp != value)
                {
                    _timeStamp = value;
                    NotifyPropertyChanged("TimeStamp");
                }
            }
        }

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
