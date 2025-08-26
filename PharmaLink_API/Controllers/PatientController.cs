using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaLink_API.Core.Enums;
using PharmaLink_API.Models.DTO.PatientDTO;
using PharmaLink_API.Repository.Interfaces;
using PharmaLink_API.Services.Interfaces;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PharmaLink_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientController : ControllerBase
    {
        private readonly IPatientService _patientService;
        private readonly IPatientRepository _patientRepository;
        public PatientController(IPatientService patientService, IPatientRepository patientRepository)
        {
            _patientService = patientService;
            _patientRepository = patientRepository;
        }

        // Correct async GET method
        [HttpGet("Profile")]
        public async Task<IActionResult> GetPatientProfile(string AccountId)
        {
            var patientInfo = await _patientService.GetPatientByIdAsync(AccountId);
            if (patientInfo == null)
            {
                return NotFound();
            }
            return Ok(patientInfo);
        }

        //// POST api/<PatientController>
        //[HttpPost]
        //public void Post([FromBody] string value)
        //{
        //}

        // PUT api/<PatientController>/5
        [HttpPut("UpdateProfile/{accountId}")]
        public async Task<IActionResult> UpdatePatientProfile([FromBody] PatientDTO patient, string accountId)
        {
            if (patient == null || string.IsNullOrWhiteSpace(accountId))
                return BadRequest();
            await _patientService.UpdatePatientAsync(patient, accountId);
            return NoContent();
        }

        // GET api/<PatientController>/5
        [HttpGet("MedicalInfo")]
        public async Task<IActionResult> GetPatientMedicalInfo(string AccountId)
        {
            if (string.IsNullOrWhiteSpace(AccountId))
                return BadRequest("AccountId is required.");
            var medicalInfo = await _patientService.GetPatientMedicalInfoByIdAsync(AccountId);
            if (medicalInfo == null)
            {
                return NotFound();
            }
            return Ok(medicalInfo);
        }
        //[Authorize(Roles = "Admin")]
        [HttpGet("AllPatients")]
        public async Task<IActionResult> GetAllPatients()
        {
            var patients = await _patientService.GetAllPatients();
            if ( patients == null)
            {
                return NotFound();
            }
            return Ok(patients);
        }

        [HttpPut("ConfirmPatient/{id:int}")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> ConfirmPatient(int id)
        {
            var patient = await _patientRepository.GetAsync(p => p.PatientId == id);
            if (patient == null)
            {
                return NotFound($"Patient with ID {id} not found.");
            }
            patient.Status = User_Status.Active;
            await _patientRepository.UpdateAsync(patient);
            return Ok($"Patient with ID {id} status updated to Active.");
        }
        [HttpPut("SuspendedPatient/{id:int}")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> SuspendedPatient(int id)
        {
            var patient = await _patientRepository.GetAsync(p => p.PatientId == id);
            if (patient == null)
            {
                return NotFound($"Patient with ID {id} not found.");
            }
            patient.Status = User_Status.Suspended;
            await _patientRepository.UpdateAsync(patient);
            return Ok($"Patient with ID {id} status updated to Suspended.");
        }

        //// DELETE api/<PatientController>/5
        //[HttpDelete("{accountId}")]
        //public async Task<IActionResult> DeletePatient(string accountId)
        //{
        //    if (string.IsNullOrWhiteSpace(accountId))
        //    {
        //        BadRequest("AccountId is required.");
        //    }
        //    _patientService.DeletePatientAsync(accountId).GetAwaiter().GetResult();
        //    // No content response for successful deletion
        //    return NoContent();
        //}
    }
}
