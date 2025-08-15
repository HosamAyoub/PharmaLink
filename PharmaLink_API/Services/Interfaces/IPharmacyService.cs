using PharmaLink_API.Models;
using PharmaLink_API.Models.DTO.PharmacyDTO;

namespace PharmaLink_API.Services.Interfaces
{
    // This interface defines the contract for pharmacy-related business logic.
    // Any service implementing this interface should provide these operations.
    public interface IPharmacyService
    {
        // Returns all pharmacies as DTOs.
        Task<IEnumerable<PharmacyDisplayDTO>> GetAllPharmaciesAsync();
        
        // Returns a single pharmacy by its ID, or null if not found.
        Task<PharmacyDisplayDTO?> GetPharmacyByIdAsync(int id);
        
        // Returns a single pharmacy by its name, or null if not found.
        Task<PharmacyDisplayDTO?> GetPharmacyByNameAsync(string name);
        
        // Returns all pharmacies that match the given name.
        Task<IEnumerable<PharmacyDisplayDTO>> GetAllPharmaciesByNameAsync(string name);
        
        // Updates a pharmacy's details. Returns true if successful, false if not found.
        Task<bool> UpdatePharmacyAsync(int id, PharmacyDisplayDTO editedPharmacy);
        
        // Deletes a pharmacy by its ID. Returns true if successful, false if not found.
        Task<bool> DeletePharmacyAsync(int id);

        Task<string?> GetAccountIdByPharmacyIdAsync(string pharmacyId);
        Task<string?> GetPharmacyIdByAccountIdAsync(string accountId);
    }
}
