using Places.Src;
using PropertyChanged;
using System;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Windows.Media.Imaging;

namespace Places.Models
{
    [Table]
    [ImplementPropertyChanged]
    public class Location : IComparable
    {
        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL Identity", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public int Id { get; set; }

        [Column]
        public string ImageName { get; set; }

        [Column]
        public string ImageUri { get; set; }

        public BitmapImage LocationImage
        {
            get { return Utilities.GetLocationImage(ImageName); }
        }

        public BitmapImage Thumbnail { get; set; }

        [Column]
        public string Name { get; set; }

        [Column]
        public double Latitude { get; set; }

        [Column]
        public double Longitude { get; set; }

        [Column]
        public double Accuracy { get; set; }

        [Column]
        public double? Distance { get; set; }

        [Column]
        public string Comment { get; set; }

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
                _locationAddress.Entity = value;

                if (value != null)
                {
                    _addressId = value.Id;
                }
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

        #endregion Foreign Keys

        [Column(IsVersion = true)]
        private Binary _version;

        public int CompareTo(object obj)
        {
            var location = obj as Location;
            return location != null ? location.Name.CompareTo(Name) : 0;
        }
    }
}