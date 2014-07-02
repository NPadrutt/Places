using System.Data.Linq;

namespace Places.Models
{
    public class MainDataContext : DataContext
    {
        public readonly int SCHEMAVERSION = 2;
        public static readonly string DBConnectionString = "Data Source=isostore:/Places.sdf";

        public MainDataContext(string connectionString)
            : base(connectionString)
        { }

        public Table<Location> Locations;
        public Table<LocationAddress> LocationAddresses;
        public Table<Tag> Tags;
        public Table<Setting> Settings;
    }
}