using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PharmaLink_API.Models;
using PharmaLink_API.Models.DTO.PharmacyDTO;
using PharmaLink_API.Repository.IRepository;

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
        [HttpGet("{id:int}")]
        public async Task<ActionResult<PharmacyDisplayDTO>> GetPharmacyById(int id)
        {
            var pharmacy = await _PharmacyRepo.GetAsync(p => p.PharmacyID == id);
            if (pharmacy == null)
            {
                return NotFound($"Pharmacy with ID {id} not found.");
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
        [HttpGet("search/{name:alpha}")]
        public async Task<ActionResult<IEnumerable<PharmacyDisplayDTO>>> GetAllPharmaciesByName(string name)
        {
            var pharmacies = await _PharmacyRepo.GetAllPharmaciesByNameAsync(name);
            if (pharmacies == null || pharmacies.Count == 0)
            {
                return NotFound($"No pharmacies found with name {name}.");
            }
            var pharmaciesDto = _Mapper.Map<IEnumerable<PharmacyDisplayDTO>>(pharmacies);
            return Ok(pharmaciesDto);
        }
        //[HttpPost]
        //public async Task<IActionResult> CreatePharmacy([FromBody] Pharmacy pharmacy)
        //{
        //    if (pharmacy == null || string.IsNullOrEmpty(pharmacy.Name))
        //    {
        //        return BadRequest("Invalid pharmacy data.");
        //    }
        //    await _PharmacyRepo.CreateAsync(pharmacy);
        //    return CreatedAtAction(nameof(GetPharmacyById), new { id = pharmacy.PharmacyID }, pharmacy);
        //}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdatePharmacy(int id, [FromBody] PharmacyDisplayDTO Editedpharmacy)
        {
            if (Editedpharmacy == null)
            {
                return BadRequest("Invalid pharmacy data.");
            }
            var existingPharmacy = await _PharmacyRepo.GetAsync(p => p.PharmacyID == id);
            if (existingPharmacy == null)
            {
                return NotFound($"Pharmacy with ID {id} not found.");
            }
            // Map the updated properties from DTO to the existing entity
            var pharmacyToUpdate = _Mapper.Map<Pharmacy>(Editedpharmacy);
            await _PharmacyRepo.UpdateAsync(pharmacyToUpdate);
            return NoContent();
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

