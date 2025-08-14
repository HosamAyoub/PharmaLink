using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PharmaLink_API.Core.Enums;
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
        private IDrugRepository _DrugRepo { get; set; }
        private IMapper _Mapper { get; set; }
        private readonly IWebHostEnvironment _WebHostEnvironment;

        public PharmacyController(IPharmacyRepository pharmacyRepo, IDrugRepository drugRepo, IMapper mapper, IWebHostEnvironment webHostEnvironment)
        {
            _PharmacyRepo = pharmacyRepo;
            _DrugRepo = drugRepo;
            _Mapper = mapper;
            _WebHostEnvironment = webHostEnvironment;
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



        [HttpGet("pharmacyById")]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<PharmacyDisplayDTO>> GetPharmacyById_forUser(int Id)
        {

            var pharmacy = await _PharmacyRepo.GetAsync(p => p.PharmacyID == Id);
            if (pharmacy == null)
            {
                return NotFound($"Pharmacy not found.");
            }

            var pharmacyDto = _Mapper.Map<PharmacyDisplayDTO>(pharmacy);
            return Ok(pharmacyDto);
        }


        [Authorize(Roles = "Pharmacy")]
        [HttpPost("SendRequestAddDrug")]
        public async Task<IActionResult> RequestToAddDrug([FromBody] SendRequestDTO drugRequest)
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountId))
                return Unauthorized( new {  message = "Invalid token." });

            var existingPharmacy = await _PharmacyRepo.GetAsync(p => p.AccountId == accountId, true, x => x.Account);
            if (existingPharmacy == null)
            {
                return NotFound(new { message = $"Pharmacy not found." });
            }
            drugRequest.CreatedByPharmacy = existingPharmacy.PharmacyID;
            drugRequest.DrugStatus = Status.Requested;
            await _DrugRepo.CreateAndSaveAsync(_Mapper.Map<Drug>(drugRequest));
            return Ok(new { message=$"Request to add drug sent successfully for pharmacy {existingPharmacy.Name}."});
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

        [HttpPut("UpdatePharmacy")]
        [Authorize(Roles = "Pharmacy")]
        public async Task<IActionResult> UpdatePharmacy([FromForm] PharmacyUpdateDTO Editedpharmacy)
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
            existingPharmacy.PhoneNumber = Editedpharmacy.PhoneNumber;
            existingPharmacy.Account.Email = Editedpharmacy.Email;
            existingPharmacy.StartHour = Editedpharmacy.StartHour;
            existingPharmacy.EndHour = Editedpharmacy.EndHour;

            if (Editedpharmacy.Photo != null && Editedpharmacy.Photo.Length > 0)
            {
                var extension = Path.GetExtension(Editedpharmacy.Photo.FileName).ToLower();
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                if (!allowedExtensions.Contains(extension))
                    return BadRequest("Unsupported image format.");

                // remove old image if it exists
                if (!string.IsNullOrEmpty(existingPharmacy.ImgUrl))
                {
                    var oldFileName = Path.GetFileName(new Uri(existingPharmacy.ImgUrl).LocalPath);
                    var oldFilePath = Path.Combine(_WebHostEnvironment.WebRootPath, "uploads", oldFileName);

                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // save new image
                var fileName = Guid.NewGuid() + extension;
                var filePath = Path.Combine(_WebHostEnvironment.WebRootPath, "uploads", fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await Editedpharmacy.Photo.CopyToAsync(stream);

                existingPharmacy.ImgUrl = $"{Request.Scheme}://{Request.Host}/uploads/{fileName}";
            }


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
        [HttpGet("GetPharmaciesByStatus/{status}")]
        //[Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<PharmacyDisplayDTO>>> GetPharmaciesByStatus(Pharmacy_Status status)
        {
            var pharmacies = await _PharmacyRepo.GetAllAsync(p => p.Status == status);
            if (pharmacies == null || pharmacies.Count == 0)
            {
                return NotFound($"No pharmacies found with status {status}.");
            }
            var pharmaciesDto = _Mapper.Map<IEnumerable<PharmacyDisplayDTO>>(pharmacies);
            return Ok(pharmaciesDto);
        }

        [HttpPut("ConfirmPharmacy/{id:int}")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> ConfirmPharmacy(int id)
        {
            var pharmacy = await _PharmacyRepo.GetAsync(p => p.PharmacyID == id);
            if (pharmacy == null)
            {
                return NotFound($"Pharmacy with ID {id} not found.");
            }
            pharmacy.Status = Pharmacy_Status.Active;
            pharmacy.JoinedDate = DateTime.Now;
            await _PharmacyRepo.UpdateAsync(pharmacy);
            return Ok($"Pharmacy with ID {id} status updated to Active.");
        }
        [HttpPut("SuspendedPharmacy/{id:int}")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> SuspendedPharmacy(int id)
        {
            var pharmacy = await _PharmacyRepo.GetAsync(p => p.PharmacyID == id);
            if (pharmacy == null)
            {
                return NotFound($"Pharmacy with ID {id} not found.");
            }
            pharmacy.Status = Pharmacy_Status.Suspended;
            await _PharmacyRepo.UpdateAsync(pharmacy);
            return Ok($"Pharmacy with ID {id} status updated to Suspended.");
        }

        [HttpPut("RejectPharmacy/{id:int}")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectPharmacy(int id)
        {
            var pharmacy = await _PharmacyRepo.GetAsync(p => p.PharmacyID == id);
            if (pharmacy == null)
            {
                return NotFound($"Pharmacy with ID {id} not found.");
            }
            pharmacy.Status = Pharmacy_Status.Rejected;
            await _PharmacyRepo.UpdateAsync(pharmacy);
            return Ok($"Pharmacy with ID {id} status updated to Rejected.");
        }
    }
}

