using PropertyChanged;
using System.ComponentModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace Places.Models
{
    [Table]
    [ImplementPropertyChanged]
    public class LocationAddress
    {
        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL Identity", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public int Id { get; set; }

        [Column]
        public string Street { get; set; }

        [Column]
        public string HouseNumber { get; set; }

        [Column]
        public string PostalCode { get; set; }

        [Column]
        public string City { get; set; }

        [Column]
        public string District { get; set; }

        [Column]
        public string State { get; set; }

        [Column]
        public string Country { get; set; }

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
            location.LocationAddress = this;
        }

        private void detach_Location(Location location)
        {
            location.LocationAddress = null;
        }

        #endregion Foreign Keys

        [Column(IsVersion = true)]
        private Binary _version;
    }
}