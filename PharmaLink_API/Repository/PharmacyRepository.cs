using PharmaLink_API.Data;
using PharmaLink_API.Models;
using PharmaLink_API.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
namespace PharmaLink_API.Repository
{
    public class PharmacyRepository : Repository<Pharmacy>, IPharmacyRepository
    {
        private readonly ApplicationDbContext _db;
        
        public PharmacyRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public async Task<Pharmacy> GetPharmacyByNameAsync(string name)
        {
            return await GetAsync(p => p.Name == name);
        }
        public async Task<List<Pharmacy>> GetAllPharmaciesByNameAsync(string name)
        {
            return await GetAllAsync(p=> p.Name == name);
        }
        public async Task DeletePharmacyByIdAsync(int id)
        {
            var pharmacy = await GetAsync(p=>p.PharmacyID==id);
            if (pharmacy != null)
            {
                await RemoveAsync(pharmacy);
            }
        }
    }
    
}
