﻿using System.Device.Location;
using System.Windows.Media;
using Microsoft.Phone.Data.Linq;
using MyTravelHistory.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTravelHistory.Src
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
                    schemaUpdate.AddColumn<Location>("ThumbnailImageName");
                }

                schemaUpdate.DatabaseSchemaVersion = db.SCHEMAVERSION;
                schemaUpdate.Execute();
            }
        }
    }
}
