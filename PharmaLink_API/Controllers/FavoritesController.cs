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

        public FavoritesController(IFavoriteRepository favoriteRepository, IPatientRepository patientRepository, IDrugRepository drugRepository)
        {
            _favoriteRepository = favoriteRepository;
            _patientRepository = patientRepository;
            _drugRepository = drugRepository;
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
                return NotFound("No favorite drugs found for this patient.");

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
                return BadRequest("Invalid favorite drug data.");

            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountId))
                return Unauthorized("User is not authenticated.");

            var patient = await _patientRepository.GetAsync(p => p.AccountId == accountId);
            if (patient == null)
                return NotFound("Patient not found.");

            var drug = await _drugRepository.GetAsync(d => d.DrugID == favorite.DrugId);
            if (drug == null)
                return NotFound("Drug not found.");

            var existingFavorite = await _favoriteRepository.GetAsync(f => f.PatientId == patient.PatientId && f.DrugId == favorite.DrugId);
            if (existingFavorite != null)
                return Conflict("This drug is already in your favorites.");

            var newFavorite = new PatientFavoriteDrug
            {
                PatientId = patient.PatientId,
                DrugId = favorite.DrugId
            };

            await _favoriteRepository.CreateAsync(newFavorite);
            await _favoriteRepository.SaveAsync();

            return Ok("Drug added to favorites successfully.");
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
