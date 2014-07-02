using Microsoft.Phone.Data.Linq;
using Places.Models;

namespace Places.Src
{
    internal class UpdateHelper
    {
        private DatabaseSchemaUpdater schemaUpdate;

        public void UpdateDatabase(MainDataContext db)
        {
            schemaUpdate = db.CreateDatabaseSchemaUpdater();
            if (schemaUpdate.DatabaseSchemaVersion < db.SCHEMAVERSION)
            {
                if (schemaUpdate.DatabaseSchemaVersion == 1)
                {
                    schemaUpdate.AddColumn<Location>("Distance");
                }

                schemaUpdate.DatabaseSchemaVersion = db.SCHEMAVERSION;
                schemaUpdate.Execute();
            }
        }
    }
}