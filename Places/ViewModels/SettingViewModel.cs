using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Places.Models;

namespace Places.ViewModels
{
    public class SettingViewModel : INotifyPropertyChanged
    {
        #region Settings and Default Definition

        const string LocationServiceEnabledKeyName = "LocationServiceEnabled";

        const bool LocationServiceEnabledKeyDefault = false;

        #endregion

        private void AddOrUpdateValue(string key, Object value)
        {
            using (var db = new MainDataContext(MainDataContext.DBConnectionString))
            {
                if (!db.Settings.Any(item => item.Key == key))
                {
                    var t = new Setting() { Key = key, Value = value.ToString() };
                    db.Settings.InsertOnSubmit(t);
                    db.SubmitChanges();
                }

                db.Settings.Single(x => x.Key == key).Value = value.ToString();
                db.SubmitChanges();
            }
        }

        private valueType GetValueOrDefault<valueType>(string key, valueType defaultValue)
        {
            valueType value;

            using (var db = new MainDataContext(MainDataContext.DBConnectionString))
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

        public bool LocationServiceEnabled
        {
            get
            {
                return GetValueOrDefault<bool>(LocationServiceEnabledKeyName, LocationServiceEnabledKeyDefault);
            }
            set
            {
                AddOrUpdateValue(LocationServiceEnabledKeyName, value);
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
