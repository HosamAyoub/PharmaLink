using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaLink_API.Models;

namespace PharmaLink_API.Repository.Interfaces
{
    // This interface defines additional data access methods for the Pharmacy entity,
    // beyond the generic repository operations.
    public interface IPharmacyRepository : IRepository<Pharmacy>
    {
        // Retrieves a single pharmacy by its name.
        Task<Pharmacy> GetPharmacyByNameAsync(string name);

        // Retrieves all pharmacies that match the given name.
        Task<List<Pharmacy>> GetAllPharmaciesByNameAsync(string name);

        // Deletes a pharmacy by its ID.
        Task DeletePharmacyByIdAsync(int id);
        public List<Pharmacy> GetPharmaciesByFilter(string filter, int size);

        DbSet<Pharmacy> GetAllPharmacies();

    }
}
