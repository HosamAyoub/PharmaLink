using PharmaLink_API.Data;
using PharmaLink_API.Models;
using PharmaLink_API.Repository.IRepository;

namespace PharmaLink_API.Repository
{
    public class PharmacyStockRepository : Repository<PharmacyStock>, IPharmacyStockRepository
    {
        private readonly ApplicationDbContext _db;
        public PharmacyStockRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

    }
}
