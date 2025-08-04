using Microsoft.AspNetCore.Mvc;
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
        [HttpGet("Info")]
        public async Task<IActionResult> GetPatientInfo(string AccountId)
        {
            var patientInfo = await _patientService.GetPatientByUserNameAsync(AccountId);
            if (patientInfo == null)
            {
                return NotFound();
            }
            return Ok(patientInfo);
        }

        // POST api/<PatientController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<PatientController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<PatientController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
