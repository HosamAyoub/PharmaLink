using Microsoft.AspNetCore.Mvc;
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
        public PatientController(IPatientService patientService)
        {
            _patientService = patientService;
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

        //// DELETE api/<PatientController>/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
