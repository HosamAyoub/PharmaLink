using PharmaLink_API.Models;

namespace PharmaLink_API.Repository.IRepository
{
    public interface IPharmacyStockRepository : IRepository<PharmacyProduct>
    {
        IEnumerable<PharmacyProduct> GetPharmacyStock(int pharmacyId, int pageNumber, int pageSize);
        public void AddProductsToPharmacyStock(List<PharmacyProduct> PharmacyProduct);
        public void UpdatePharmacyProduct(PharmacyProduct PharmacyProduct);
        public PharmacyProduct? GetPharmacyProduct(int pharmacyId, int drugId);
        public void DeletePharmacyProduct(PharmacyProduct PharmacyProduct);
    }
    
}
