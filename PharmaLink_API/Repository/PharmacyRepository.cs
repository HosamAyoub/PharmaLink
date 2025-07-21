using PharmaLink_API.Data;
using PharmaLink_API.Models;
using PharmaLink_API.Repository.IRepository;

namespace PharmaLink_API.Repository
{
    public class PharmacyRepository : Repository<Pharmacy>, IPharmacyRepository
    {
        private readonly ApplicationDbContext _db;
        public PharmacyRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
    }
}
