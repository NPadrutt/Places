using System.ComponentModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace Places.Models
{
    [Table]
    public class Tag : INotifyPropertyChanged
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

        private string _tagName;

        [Column]
        public string TagName
        {
            get { return _tagName; }
            set
            {
                if (_tagName != value)
                {
                    _tagName = value;
                    NotifyPropertyChanged("TagName");
                }
            }
        }

        #region Foreign Keys

        [Column]
        private int? _locationId;

        private EntityRef<Location> _location;

        [Association(Storage = "_location", ThisKey = "_locationId", OtherKey = "Id", IsForeignKey = true)]
        public Location Location
        {
            get { return _location.Entity; }
            set
            {
                NotifyPropertyChanging("Location");
                _location.Entity = value;

                if (value != null)
                {
                    _locationId = value.Id;
                }

                NotifyPropertyChanging("Location");
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
    }
}
