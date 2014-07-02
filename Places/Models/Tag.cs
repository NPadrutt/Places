using PropertyChanged;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace Places.Models
{
    [Table]
    [ImplementPropertyChanged]
    public class Tag
    {
        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL Identity", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public int Id { get; set; }

        [Column]
        public string TagName { get; set; }

        #region Foreign Keys

        private int? _locationId;

        private EntityRef<Location> _location;

        [Association(Storage = "_location", ThisKey = "_locationId", OtherKey = "Id", IsForeignKey = true)]
        public Location Location
        {
            get { return _location.Entity; }
            set
            {
                _location.Entity = value;

                if (value != null)
                {
                    _locationId = value.Id;
                }
            }
        }

        #endregion Foreign Keys

        [Column(IsVersion = true)]
        private Binary _version;
    }
}