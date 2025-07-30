using Microsoft.AspNetCore.Mvc;
using PharmaLink_API.Models.DTO.PharmacyDTO;
using PharmaLink_API.Services.Interfaces;

namespace PharmaLink_API.Controllers
{
    // This controller handles all HTTP requests related to pharmacies.
    // It delegates business logic to the IPharmacyService.
    [Route("api/[controller]")]
    [ApiController]
    public class PharmacyController : ControllerBase
    {
        private readonly IPharmacyService _pharmacyService;

        // The service is injected via dependency injection.
        public PharmacyController(IPharmacyService pharmacyService)
        {
            _pharmacyService = pharmacyService;
        }

        // GET: api/pharmacy
        // Returns a list of all pharmacies.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PharmacyDisplayDTO>>> GetAllPharmacies()
        {
            var pharmacies = await _pharmacyService.GetAllPharmaciesAsync();
            if (pharmacies == null || !pharmacies.Any())
                return NotFound("No pharmacies found.");
            return Ok(pharmacies);
        }

        // GET: api/pharmacy/{id}
        // Returns a single pharmacy by its unique ID.
        [HttpGet("{id:int}")]
        public async Task<ActionResult<PharmacyDisplayDTO>> GetPharmacyById(int id)
        {
            var pharmacy = await _pharmacyService.GetPharmacyByIdAsync(id);
            if (pharmacy == null)
                return NotFound($"Pharmacy with ID {id} not found.");
            return Ok(pharmacy);
        }

        // GET: api/pharmacy/{name}
        // Returns a single pharmacy by its name.
        [HttpGet("{name:alpha}")]
        public async Task<ActionResult<PharmacyDisplayDTO>> GetPharmacyByName(string name)
        {
            var pharmacy = await _pharmacyService.GetPharmacyByNameAsync(name);
            if (pharmacy == null)
                return NotFound($"Pharmacy with name {name} not found.");
            return Ok(pharmacy);
        }

        // GET: api/pharmacy/search/{name}
        // Returns all pharmacies that match the given name.
        [HttpGet("search/{name:alpha}")]
        public async Task<ActionResult<IEnumerable<PharmacyDisplayDTO>>> GetAllPharmaciesByName(string name)
        {
            var pharmacies = await _pharmacyService.GetAllPharmaciesByNameAsync(name);
            if (pharmacies == null || !pharmacies.Any())
                return NotFound($"No pharmacies found with name {name}.");
            return Ok(pharmacies);
        }

        // PUT: api/pharmacy/{id}
        // Updates the details of an existing pharmacy.
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdatePharmacy(int id, [FromBody] PharmacyDisplayDTO editedPharmacy)
        {
            if (editedPharmacy == null)
                return BadRequest("Invalid pharmacy data.");

            var updated = await _pharmacyService.UpdatePharmacyAsync(id, editedPharmacy);
            if (!updated)
                return NotFound($"Pharmacy with ID {id} not found.");

            return NoContent();
        }

        // DELETE: api/pharmacy/{id}
        // Deletes a pharmacy by its ID.
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeletePharmacy(int id)
        {
            var deleted = await _pharmacyService.DeletePharmacyAsync(id);
            if (!deleted)
                return NotFound($"Pharmacy with ID {id} not found.");

            return Ok($"Pharmacy with ID {id} deleted successfully.");
        }
    }
}

