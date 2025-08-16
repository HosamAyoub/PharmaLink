using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PharmaLink_API.Services.Interfaces;

namespace PharmaLink_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SharedController : ControllerBase
    {
        private readonly ISharedService sharedService;

        public SharedController(ISharedService sharedService)
        {
            this.sharedService = sharedService;
        }

        [HttpGet("searchByDrugOrPharmacy")]
        public IActionResult serachByDrugOrPharamcy(string filter, int size = 4)
        {
            if (size < 1)
            {
                return BadRequest();
            }
            if (filter == null)
            {
                return NotFound();
            }

            try
            {
                var result = sharedService.GetPharmaciesAndDrugsByFilter(filter, size);
                return Ok(result);
            }
            catch
            {
                return StatusCode(500, "something wrong happend");
            }
        }
    }
}
