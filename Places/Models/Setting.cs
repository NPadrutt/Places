using PropertyChanged;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace Places.Models
{
    [Table]
    [ImplementPropertyChanged]
    public class Setting
    {
        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL Identity", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public int Id { get; set; }

        [Column]
        public string Key { get; set; }

        [Column]
        public string Value { get; set; }

        [Column(IsVersion = true)]
        private Binary _version;
    }
}