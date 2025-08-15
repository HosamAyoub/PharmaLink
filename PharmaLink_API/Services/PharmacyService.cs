using AutoMapper;
using PharmaLink_API.Models;
using PharmaLink_API.Models.DTO.PharmacyDTO;
using PharmaLink_API.Repository.Interfaces;
using PharmaLink_API.Services.Interfaces;

namespace PharmaLink_API.Services
{
    // This service contains the business logic for managing pharmacies.
    // It interacts with the repository and handles mapping between entities and DTOs.
    public class PharmacyService : IPharmacyService
    {
        private readonly IPharmacyRepository _pharmacyRepo;
        private readonly IMapper _mapper;

        // Injects the repository and mapper via dependency injection.
        public PharmacyService(IPharmacyRepository pharmacyRepo, IMapper mapper)
        {
            _pharmacyRepo = pharmacyRepo;
            _mapper = mapper;
        }

        // Retrieves all pharmacies from the database and maps them to DTOs.
        public async Task<IEnumerable<PharmacyDisplayDTO>> GetAllPharmaciesAsync()
        {
            var pharmacies = await _pharmacyRepo.GetAllAsync();
            return _mapper.Map<IEnumerable<PharmacyDisplayDTO>>(pharmacies);
        }

        // Retrieves a single pharmacy by its ID and maps it to a DTO.
        public async Task<PharmacyDisplayDTO?> GetPharmacyByIdAsync(int id)
        {
            var pharmacy = await _pharmacyRepo.GetAsync(p => p.PharmacyID == id);
            return pharmacy == null ? null : _mapper.Map<PharmacyDisplayDTO>(pharmacy);
        }

        // Retrieves a single pharmacy by its name and maps it to a DTO.
        public async Task<PharmacyDisplayDTO?> GetPharmacyByNameAsync(string name)
        {
            var pharmacy = await _pharmacyRepo.GetPharmacyByNameAsync(name);
            return pharmacy == null ? null : _mapper.Map<PharmacyDisplayDTO>(pharmacy);
        }

        // Retrieves all pharmacies with a matching name and maps them to DTOs.
        public async Task<IEnumerable<PharmacyDisplayDTO>> GetAllPharmaciesByNameAsync(string name)
        {
            var pharmacies = await _pharmacyRepo.GetAllPharmaciesByNameAsync(name);
            return _mapper.Map<IEnumerable<PharmacyDisplayDTO>>(pharmacies);
        }

        /// <summary>
        /// Converts a pharmacy database ID to its corresponding Account ID (GUID).
        /// </summary>
        /// <param name="pharmacyId">The pharmacy's database ID</param>
        /// <returns>The Account ID (GUID) if found, null otherwise</returns>
        public async Task<string?> GetAccountIdByPharmacyIdAsync(string pharmaId)
        {
            try
            {
                int pharmacyId = int.TryParse(pharmaId, out var id) ? id : -1;

                if (pharmacyId <= 0)
                {
                    return null;
                }

                var pharmacy = await _pharmacyRepo.GetAsync(
                    filter: p => p.PharmacyID == pharmacyId,
                    tracking: false,
                    includeProperties: p => p.Account
                );

                return pharmacy?.AccountId;
            }
            catch (Exception)
            {
                // Log error if needed
                return null;
            }
        }

        /// <summary>
        /// Converts an Account ID (GUID) to its corresponding pharmacy database ID.
        /// </summary>
        /// <param name="accountId">The Account ID (GUID)</param>
        /// <returns>The pharmacy database ID if found, null otherwise</returns>
        public async Task<string?> GetPharmacyIdByAccountIdAsync(string accountId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(accountId))
                {
                    return null;
                }

                var pharmacy = await _pharmacyRepo.GetAsync(
                    filter: p => p.AccountId == accountId,
                    tracking: false
                );

                return pharmacy?.PharmacyID.ToString();
            }
            catch (Exception)
            {
                // Log error if needed
                return null;
            }
        }

        // Updates an existing pharmacy's details.
        // Returns true if the update was successful, false if the pharmacy was not found.
        public async Task<bool> UpdatePharmacyAsync(int id, PharmacyDisplayDTO editedPharmacy)
        {
            var existingPharmacy = await _pharmacyRepo.GetAsync(p => p.PharmacyID == id);
            if (existingPharmacy == null) return false;

            var pharmacyToUpdate = _mapper.Map<Pharmacy>(editedPharmacy);
            pharmacyToUpdate.PharmacyID = id; // Ensure the ID is preserved
            await _pharmacyRepo.UpdateAsync(pharmacyToUpdate);
            return true;
        }

        // Deletes a pharmacy by its ID.
        // Returns true if the deletion was successful, false if the pharmacy was not found.
        public async Task<bool> DeletePharmacyAsync(int id)
        {
            var pharmacy = await _pharmacyRepo.GetAsync(p => p.PharmacyID == id);
            if (pharmacy == null) return false;

            await _pharmacyRepo.RemoveAsync(pharmacy);
            return true;
        }
    }
}
