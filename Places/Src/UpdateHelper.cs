using Microsoft.Phone.Data.Linq;
using Places.Models;

namespace Places.Src
{
    class UpdateHelper
    {
        DatabaseSchemaUpdater schemaUpdate;

        public void UpdateDatabase(MainDataContext db)
        {
            schemaUpdate = db.CreateDatabaseSchemaUpdater();
            if (schemaUpdate.DatabaseSchemaVersion < db.SCHEMAVERSION)
            {
                if (schemaUpdate.DatabaseSchemaVersion == 1)
                {
                }

                schemaUpdate.DatabaseSchemaVersion = db.SCHEMAVERSION;
                schemaUpdate.Execute();
            }
        }
    }
}
