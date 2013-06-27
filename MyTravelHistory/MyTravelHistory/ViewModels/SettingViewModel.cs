using MyTravelHistory.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTravelHistory.ViewModels
{
    public class SettingViewModel : INotifyPropertyChanged
    {
        #region Settings and Default Definition

        const string LATITUDE_KEYNAME = "Latitude";
        const string LONGTITUDE_KEYNAME = "Longtitude";

        const double LATITUDE_KEYDEFAULT = 0;
        const double LONGTITUDE_KEYDEFAULT = 0;
        
        #endregion

        public void AddOrUpdateValue(string key, Object value)
        {
            using (MainDataContext db = new MainDataContext(MainDataContext.DBConnectionString))
            {
                if (!db.Settings.Any(item => item.Key == key))
                {
                    var t = new Setting() { Key = key, Value = value.ToString() };
                    db.Settings.InsertOnSubmit(t);
                    db.SubmitChanges();
                }
            }
        }

        public valueType GetValueOrDefault<valueType>(string key, valueType defaultValue)
        {
            valueType value;

            using (MainDataContext db = new MainDataContext(MainDataContext.DBConnectionString))
            {
                if (db.Settings.Any(item => item.Key == key))
                {
                    var selectedItem = db.Settings.Single(item => item.Key == key);
                    value = (valueType)Convert.ChangeType(selectedItem.Value, typeof(valueType), CultureInfo.InvariantCulture);
                }
                else
                {
                    value = defaultValue;
                }
            }

            return value;
        }

        public double Latitude
        {
            get
            {
                return GetValueOrDefault<double>(LATITUDE_KEYNAME, LATITUDE_KEYDEFAULT);
            }
            set
            {
                AddOrUpdateValue(LATITUDE_KEYNAME, value);
            }
        }

        public double Longtitude
        {
            get
            {
                return GetValueOrDefault<double>(LONGTITUDE_KEYNAME, LONGTITUDE_KEYDEFAULT);
            }
            set
            {
                AddOrUpdateValue(LONGTITUDE_KEYNAME, value);
            }
        }

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
