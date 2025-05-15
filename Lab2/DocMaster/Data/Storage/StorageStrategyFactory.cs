using DocMaster.Services.FileService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocMaster.Data.StorageStrategies
{
    public class StorageStrategyFactory
    {
        public static IStorageStrategy CreateStrategy(
            StorageType type,
            string storagePath = null,
            string dbConnectionString = null)
        {
            return type switch
            {
                StorageType.Local => new LocalStorageStrategy(storagePath),
                StorageType.Database => new DatabaseStorageStrategy(),
                StorageType.Cloud => new CloudStorageStrategy(),
                _ => throw new NotSupportedException()
            };
        }
    }
}
