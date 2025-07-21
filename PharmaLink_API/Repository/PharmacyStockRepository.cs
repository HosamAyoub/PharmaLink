using Microsoft.EntityFrameworkCore;
using PharmaLink_API.Data;
using PharmaLink_API.Models;
using PharmaLink_API.Repository.IRepository;

namespace PharmaLink_API.Repository
{
    public class PharmacyStockRepository : Repository<PharmacyProduct>, IPharmacyStockRepository
    {
        private readonly ApplicationDbContext db;

        public PharmacyStockRepository(ApplicationDbContext db) : base(db)
        {
            this.db = db;
        }

        public IEnumerable<PharmacyProduct> GetPharmacyStock(int pharmacyId , int pageNumber , int pageSize)
        {
            var pharmacyStock = db.PharmacyStock
                .Where(ps => ps.PharmacyId == pharmacyId)
                .Include(ps => ps.Drug)
                .Include(ps => ps.Pharmacy); 
            if (pageNumber > 0 && pageSize > 0)
            {
                return pharmacyStock.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            }
            else
            {
                return pharmacyStock.ToList();
            }
        }

        public PharmacyProduct? GetPharmacyProduct(int pharmacyId, int drugId)
        {
           return db.PharmacyStock.AsNoTracking().FirstOrDefault(ps => ps.PharmacyId == pharmacyId && ps.DrugId == drugId);
        }

        public void AddProductsToPharmacyStock(List<PharmacyProduct> pharmacyStock)
        {
            db.PharmacyStock.AddRange(pharmacyStock);
            db.SaveChanges();
        }

        public void UpdatePharmacyProduct(PharmacyProduct pharmacyStock)
        {
            db.PharmacyStock.Update(pharmacyStock);
            db.SaveChanges();
        }

        public void DeletePharmacyProduct(PharmacyProduct pharmacyStock)
        {
            db.PharmacyStock.Remove(pharmacyStock);
            db.SaveChanges();
        }

    }
}
