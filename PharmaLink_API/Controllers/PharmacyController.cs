using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PharmaLink_API.Models;
using PharmaLink_API.Repository.IRepository;

namespace PharmaLink_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PharmacyController : ControllerBase
    {
        public IPharmacyRepository PharmacyRepo { get; set; }
        public PharmacyController(IPharmacyRepository pharmacyRepo)
        {
            PharmacyRepo = pharmacyRepo;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllPharmacies()
        {
            var pharmacies = await PharmacyRepo.GetAllAsync();
            if (pharmacies == null || pharmacies.Count == 0)
            {
                return NotFound("No pharmacies found.");
            }
            return Ok(pharmacies);
        }
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetPharmacyById(int id)
        {
            var pharmacy = await PharmacyRepo.GetAsync(p => p.PharmacyID == id);
            if (pharmacy == null)
            {
                return NotFound($"Pharmacy with ID {id} not found.");
            }
            return Ok(pharmacy);
        }
        [HttpGet("{name:alpha}")]
        public async Task<IActionResult> GetPharmacyByName(string name)
        {
            var pharmacy = await PharmacyRepo.GetPharmacyByNameAsync(name);
            if (pharmacy == null)
            {
                return NotFound($"Pharmacy with name {name} not found.");
            }
            return Ok(pharmacy);
        }
        [HttpPost]
        public async Task<IActionResult> CreatePharmacy([FromBody] Pharmacy pharmacy)
        {
            if (pharmacy == null || string.IsNullOrEmpty(pharmacy.Name))
            {
                return BadRequest("Invalid pharmacy data.");
            }
            await PharmacyRepo.CreateAsync(pharmacy);
            return CreatedAtAction(nameof(GetPharmacyById), new { id = pharmacy.PharmacyID }, pharmacy);
        }
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdatePharmacy(int id, [FromBody] Pharmacy Editedpharmacy)
        {
            if (Editedpharmacy == null || Editedpharmacy.PharmacyID != id)
            {
                return BadRequest("Invalid pharmacy data.");
            }
            var existingPharmacy = await PharmacyRepo.GetAsync(p => p.PharmacyID == id);
            if (existingPharmacy == null)
            {
                return NotFound($"Pharmacy with ID {id} not found.");
            }
            await PharmacyRepo.UpdateAsync(Editedpharmacy);
            return NoContent();
        }
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeletePharmacy(int id)
        {
            var pharmacy = await PharmacyRepo.GetAsync(p => p.PharmacyID == id);
            if (pharmacy == null)
            {
                return NotFound($"Pharmacy with ID {id} not found.");
            }
            await PharmacyRepo.RemoveAsync(pharmacy);
            return Ok($"Department with ID {id} deleted successfully.");
        }
    }
}

