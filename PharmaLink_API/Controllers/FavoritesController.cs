using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PharmaLink_API.Models;
using PharmaLink_API.Models.DTO.FavoriteDTO;
using PharmaLink_API.Repository.Interfaces;
using PharmaLink_API.Repository.IRepository;
using System.Security.Claims;

namespace PharmaLink_API.Controllers
{
    [Authorize(Roles = "Patient")]
    [Route("api/[controller]")]
    [ApiController]
    public class FavoritesController : ControllerBase
    {
        private readonly IFavoriteRepository _favoriteRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IDrugRepository _drugRepository;
        private readonly ILogger<FavoritesController> logger;

        public FavoritesController(IFavoriteRepository favoriteRepository, 
            IPatientRepository patientRepository, 
            IDrugRepository drugRepository,
            ILogger<FavoritesController> logger)
        {
            _favoriteRepository = favoriteRepository;
            _patientRepository = patientRepository;
            _drugRepository = drugRepository;
            this.logger = logger;
        }

        /// <summary>
        /// Retrieves the list of favorite drugs for the authenticated patient.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetFavorites()
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountId))
                return Unauthorized("User is not authenticated.");

            var patient = await _patientRepository.GetAsync(p => p.AccountId == accountId);
            if (patient == null)
                return NotFound("Patient not found.");

            var favorites = await _favoriteRepository.GetAllAsync(f => f.PatientId == patient.PatientId, x => x.Drug);
            if (favorites == null || !favorites.Any())
                return Ok(new { message = "No favorite drugs found for this patient." });

            var favoriteDrugs = favorites.Select(f => new FavoriteDrugDTO
            {
                DrugId = f.DrugId,
                Name = f.Drug?.CommonName,
                Description = f.Drug?.Description,
                ImageUrl = f.Drug?.Drug_UrlImg,
                DrugCategory = f.Drug?.Category
            }).ToList();

            return Ok(favoriteDrugs);
        }

        /// <summary>
        /// Adds a drug to the authenticated patient's favorites.
        /// </summary>
        /// <param name="favorite">DTO containing the DrugId to add to favorites.</param>
        [HttpPost]
        public async Task<IActionResult> AddFavorite([FromBody] AddToFavoriteDTO favorite)
        {
            if (favorite == null || favorite.DrugId == 0)
                return BadRequest(new { message = "Invalid favorite drug data." });

            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountId))
                return Unauthorized(new { message = "User is not authenticated." });

            var patient = await _patientRepository.GetAsync(p => p.AccountId == accountId);
            if (patient == null)
                return NotFound("Patient not found.");

            var drug = await _drugRepository.GetAsync(d => d.DrugID == favorite.DrugId);
            if (drug == null)
                return NotFound(new { message = "Drug not found." });

            var existingFavorite = await _favoriteRepository.GetAsync(f => f.PatientId == patient.PatientId && f.DrugId == favorite.DrugId);
            if (existingFavorite != null)
                return Conflict(new { message = "This drug is already in your favorites." });

            var newFavorite = new PatientFavoriteDrug
            {
                PatientId = patient.PatientId,
                DrugId = favorite.DrugId
            };

            await _favoriteRepository.CreateAndSaveAsync(newFavorite);
            await _favoriteRepository.SaveAsync();

            return Ok(new { message = "Drug added to favorites successfully." });
        }

        /// <summary>
        /// Adds multiple drugs to the authenticated patient's favorites.
        /// </summary>
        /// <param name="favorites">DTO containing multiple drug IDs to add to favorites.</param>
        /// <returns>Response with details about successfully added favorites and any errors.</returns>
        [HttpPost("AddMultiple")]
        public async Task<IActionResult> AddMultipleFavorites([FromBody] List<int> favorites)
        {
            
            logger.LogInformation("AddMultipleFavorites called with {Count} drug IDs", favorites?.Count ?? 0);
            if (favorites == null || favorites == null || !favorites.Any())
                return BadRequest(new { message = "No drug IDs provided to add to favorites." });

            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountId))
                return Unauthorized(new { message = "User is not authenticated." });

            var patient = await _patientRepository.GetAsync(p => p.AccountId == accountId);
            if (patient == null)
                return NotFound(new { message = "Patient not found." });

            // Validate drug IDs from the list
            var drugIds = new List<int>();
            var invalidInputs = new List<string>();

            foreach (var drugId in favorites)
            {
                if (drugId <= 0)
                {
                    invalidInputs.Add($"Invalid DrugId: {drugId}. DrugId must be greater than 0.");
                    continue;
                }
                
                drugIds.Add(drugId);
            }

            var response = new AddMultipleFavoritesResponseDTO
            {
                TotalRequested = favorites.Count
            };

            // Add input validation errors to response
            if (invalidInputs.Any())
            {
                response.Errors.AddRange(invalidInputs);
                response.Failed += invalidInputs.Count;
            }

            try
            {
                // Remove duplicates from input
                var uniqueDrugIds = drugIds.Distinct().ToList();
                
                // Validate all drug IDs exist
                var existingDrugs = await _drugRepository.GetAllAsync(d => uniqueDrugIds.Contains(d.DrugID));
                var existingDrugIds = existingDrugs.Select(d => d.DrugID).ToHashSet();
                
                // Find invalid drug IDs
                var invalidDrugIds = uniqueDrugIds.Where(id => !existingDrugIds.Contains(id)).ToList();
                foreach (var invalidId in invalidDrugIds)
                {
                    response.Errors.Add($"Drug with ID {invalidId} not found.");
                    response.Failed++;
                }

                // Get existing favorites for this patient
                var existingFavorites = await _favoriteRepository.GetAllAsync(
                    f => f.PatientId == patient.PatientId && uniqueDrugIds.Contains(f.DrugId));
                var existingFavoriteDrugIds = existingFavorites.Select(f => f.DrugId).ToHashSet();

                // Identify drugs already in favorites
                var alreadyFavoriteDrugIds = uniqueDrugIds.Where(id => existingFavoriteDrugIds.Contains(id)).ToList();
                response.SkippedDrugIds.AddRange(alreadyFavoriteDrugIds);
                response.AlreadyInFavorites = alreadyFavoriteDrugIds.Count;

                // Find drugs to add (valid and not already in favorites)
                var drugsToAdd = uniqueDrugIds
                    .Where(id => existingDrugIds.Contains(id) && !existingFavoriteDrugIds.Contains(id))
                    .ToList();

                if (drugsToAdd.Any())
                {
                    // Create favorite entities
                    var newFavorites = drugsToAdd.Select(drugId => new PatientFavoriteDrug
                    {
                        PatientId = patient.PatientId,
                        DrugId = drugId
                    }).ToList();

                    // Add to database
                    await _favoriteRepository.AddRangeAsync(newFavorites);

                    // Create response DTOs for successfully added favorites
                    var addedDrugs = existingDrugs.Where(d => drugsToAdd.Contains(d.DrugID));
                    response.AddedFavorites = addedDrugs.Select(drug => new FavoriteDrugDTO
                    {
                        DrugId = drug.DrugID,
                        Name = drug.CommonName,
                        Description = drug.Description,
                        ImageUrl = drug.Drug_UrlImg,
                        DrugCategory = drug.Category
                    }).ToList();

                    response.SuccessfullyAdded = drugsToAdd.Count;
                }

                // Add informational messages for skipped items
                if (response.AlreadyInFavorites > 0)
                {
                    response.Errors.Add($"{response.AlreadyInFavorites} drug(s) were already in your favorites and were skipped.");
                }

                // Determine response based on results
                if (response.SuccessfullyAdded > 0 && response.Failed == 0)
                {
                    return Ok(new { 
                        message = "All valid drugs added to favorites successfully.", 
                        data = response 
                    });
                }
                else if (response.SuccessfullyAdded > 0 && response.Failed > 0)
                {
                    return Ok(new { 
                        message = "Some drugs were added to favorites with errors.", 
                        data = response,
                        warnings = response.Errors 
                    });
                }
                else if (response.SuccessfullyAdded == 0 && response.AlreadyInFavorites > 0 && response.Failed == 0)
                {
                    return Ok(new { 
                        message = "All drugs were already in your favorites.", 
                        data = response 
                    });
                }
                else
                {
                    return BadRequest(new { 
                        message = "Failed to add any drugs to favorites.", 
                        data = response,
                        errors = response.Errors 
                    });
                }
            }
            catch (Exception ex)
            {
                response.Errors.Add($"An error occurred while adding drugs to favorites: {ex.Message}");
                return StatusCode(500, new { 
                    message = "Internal server error.", 
                    data = response,
                    errors = response.Errors 
                });
            }
        }

        /// <summary>
        /// Removes a specific drug from the authenticated patient's favorites.
        /// </summary>
        /// <param name="drugId">The ID of the drug to remove from favorites.</param>
        [HttpDelete("{drugId}")]
        public async Task<IActionResult> RemoveFavorite(int drugId)
        {
            if (drugId <= 0)
                return BadRequest("Invalid drug ID.");

            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountId))
                return Unauthorized("User is not authenticated.");

            var patient = await _patientRepository.GetAsync(p => p.AccountId == accountId);
            if (patient == null)
                return NotFound("Patient not found.");

            var favorite = await _favoriteRepository.GetAsync(f => f.PatientId == patient.PatientId && f.DrugId == drugId);
            if (favorite == null)
                return NotFound("Favorite drug not found.");

            await _favoriteRepository.RemoveAsync(favorite);
            await _favoriteRepository.SaveAsync();

            return Ok(new { message = "Drug removed from favorites successfully." });

        }

        /// <summary>
        /// Removes all favorite drugs for the authenticated patient.
        /// </summary>
        [HttpDelete("ClearFavorites")]
        public async Task<IActionResult> ClearFavorites()
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountId))
                return Unauthorized("User is not authenticated.");

            var patient = await _patientRepository.GetAsync(p => p.AccountId == accountId);
            if (patient == null)
                return NotFound("Patient not found.");

            var favorites = await _favoriteRepository.GetAllAsync(f => f.PatientId == patient.PatientId);
            if (favorites == null || !favorites.Any())
                return NotFound("No favorite drugs found for this patient.");

            await _favoriteRepository.RemoveRange(favorites);
            await _favoriteRepository.SaveAsync();

            return Ok(new { message = "All favorite drugs removed successfully." });
        }
    }
}
