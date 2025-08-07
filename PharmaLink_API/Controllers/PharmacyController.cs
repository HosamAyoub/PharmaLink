using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PharmaLink_API.Models;
using PharmaLink_API.Models.DTO.PharmacyDTO;
using PharmaLink_API.Repository.Interfaces;
using System.Security.Claims;

namespace PharmaLink_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PharmacyController : ControllerBase
    {
        private IPharmacyRepository _PharmacyRepo { get; set; }
        private IMapper _Mapper { get; set; }

        public PharmacyController(IPharmacyRepository pharmacyRepo, IMapper mapper)
        {
            _PharmacyRepo = pharmacyRepo;
            _Mapper = mapper;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PharmacyDisplayDTO>>> GetAllPharmacies()
        {
            var pharmacies = await _PharmacyRepo.GetAllAsync();
            if (pharmacies == null || pharmacies.Count == 0)
            {
                return NotFound("No pharmacies found.");
            }
            var pharmaciesDto = _Mapper.Map<IEnumerable<PharmacyDisplayDTO>>(pharmacies);
            return Ok(pharmaciesDto);
        }

        [Authorize(Roles = "Pharmacy")]
        [HttpGet("pharmacyProfile")]
        public async Task<ActionResult<PharmacyDisplayDTO>> GetPharmacyById()
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountId))
                return Unauthorized("Invalid token.");

            var pharmacy = await _PharmacyRepo.GetAsync(p => p.AccountId == accountId, true, x => x.Account);
            if (pharmacy == null)
            {
                return NotFound($"Pharmacy not found.");
            }

            var pharmacyDto = _Mapper.Map<PharmacyDisplayDTO>(pharmacy);
            return Ok(pharmacyDto);
        }

        [HttpGet("{name:alpha}")]
        public async Task<ActionResult<PharmacyDisplayDTO>> GetPharmacyByName(string name)
        {
            var pharmacy = await _PharmacyRepo.GetPharmacyByNameAsync(name);
            if (pharmacy == null)
            {
                return NotFound($"Pharmacy with name {name} not found.");
            }
            var pharmacyDto = _Mapper.Map<PharmacyDisplayDTO>(pharmacy);
            return Ok(pharmacyDto);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePharmacy([FromBody] Pharmacy pharmacy)
        {
            if (pharmacy == null || string.IsNullOrEmpty(pharmacy.Name))
            {
                return BadRequest("Invalid pharmacy data.");
            }
            await _PharmacyRepo.CreateAndSaveAsync(pharmacy);
            return CreatedAtAction(nameof(GetPharmacyById), new { id = pharmacy.PharmacyID }, pharmacy);
        }

        [HttpPut("UpdatePharmacy")]
        public async Task<IActionResult> UpdatePharmacy([FromBody] PharmacyDisplayDTO Editedpharmacy)
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountId))
                return Unauthorized("Invalid token.");

            var existingPharmacy = await _PharmacyRepo.GetAsync(p => p.AccountId == accountId, true, x => x.Account);
            if (existingPharmacy == null)
            {
                return NotFound($"Pharmacy not found.");
            }

            if (Editedpharmacy == null)
            {
                return BadRequest("Invalid pharmacy data.");
            }
            // Update only allowed fields
            existingPharmacy.Name = Editedpharmacy.Name;
            existingPharmacy.Address = Editedpharmacy.Address;
            existingPharmacy.Account.PhoneNumber = Editedpharmacy.PhoneNumber;
            existingPharmacy.Account.Email = Editedpharmacy.Email;
            existingPharmacy.ImgUrl = Editedpharmacy.ImgUrl;
            existingPharmacy.StartHour = Editedpharmacy.StartHour;
            existingPharmacy.EndHour = Editedpharmacy.EndHour;

            await _PharmacyRepo.UpdateAsync(existingPharmacy);

            // Map updated entity back to DTO and return it
            var updatedDTO = _Mapper.Map<PharmacyDisplayDTO>(existingPharmacy);
            return Ok(updatedDTO);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeletePharmacy(int id)
        {
            var pharmacy = await _PharmacyRepo.GetAsync(p => p.PharmacyID == id);
            if (pharmacy == null)
            {
                return NotFound($"Pharmacy with ID {id} not found.");
            }
            await _PharmacyRepo.RemoveAsync(pharmacy);
            return Ok($"Department with ID {id} deleted successfully.");
        }
    }
}

