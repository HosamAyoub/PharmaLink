using PharmaLink_API.Data;
using PharmaLink_API.Models;
using PharmaLink_API.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace PharmaLink_API.Repository
{
    // This repository provides data access methods specific to the Pharmacy entity.
    // It extends the generic Repository<T> for common CRUD operations and implements IPharmacyRepository for pharmacy-specific queries.
    public class PharmacyRepository : Repository<Pharmacy>, IPharmacyRepository
    {
        private readonly ApplicationDbContext _db;

        // The ApplicationDbContext is injected to interact with the database.
        public PharmacyRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        // Retrieves a single pharmacy by its unique name.
        // Returns null if no pharmacy with the given name exists.
        public async Task<Pharmacy> GetPharmacyByNameAsync(string name)
        {
            return await GetAsync(p => p.Name == name);
        }

        // Retrieves all pharmacies that match the given name.
        // Returns an empty list if no pharmacies are found.
        public async Task<List<Pharmacy>> GetAllPharmaciesByNameAsync(string name)
        {
            return await GetAllAsync(p => p.Name == name);
        }

        // Deletes a pharmacy by its ID if it exists.
        // If the pharmacy is not found, nothing happens.
        public async Task DeletePharmacyByIdAsync(int id)
        {
            var pharmacy = await GetAsync(p => p.PharmacyID == id);
            if (pharmacy != null)
            {
                await RemoveAsync(pharmacy);
            }
        }
    }
}
