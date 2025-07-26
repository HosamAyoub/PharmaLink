using PharmaLink_API.Models;

namespace PharmaLink_API.Repository.IRepository
{
    public interface IPharmacyRepository : IRepository<Pharmacy>
    {
        Task<Pharmacy> GetPharmacyByNameAsync(string name);
        Task<List<Pharmacy>> GetAllPharmaciesByNameAsync(string name);
        //Task<List<Pharmacy>> GetPharmaciesByLocationAsync(string location);
        //Task UpdatePharmacyAsync(Pharmacy pharmacy);
        Task DeletePharmacyByIdAsync(int id);
    }
}
