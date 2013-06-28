using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTravelHistory.Models
{
    public class MainDataContext: DataContext
    {
        public readonly int SCHEMAVERSION = 2;
        public static readonly string DBConnectionString = "Data Source=isostore:/MyTravelHistory.sdf";

        public MainDataContext(string connectionString)
            : base(connectionString)
        { }

        public Table<Location> Locations;
        public Table<LocationAddress> LocationAddresses;
    }
}
