using DocMaster.Models;

namespace DocMaster.Data.StorageStrategies
{
    public interface IStorageStrategy
    {
        void Save(Document document, DocumentFormat format);
    }
}
