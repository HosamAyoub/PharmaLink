using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaLink_API.Core.Enums;
using PharmaLink_API.Core.Results;
using PharmaLink_API.Models;
using PharmaLink_API.Models.DTO.PharmacyStockDTO;
using PharmaLink_API.Services.Interfaces;

namespace PharmaLink_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PharmacyStockController : ControllerBase
    {
        private readonly IPharmacyStockService _pharmacyStockService;
        private readonly ILogger<PharmacyStockController> _logger;

        public PharmacyStockController(IPharmacyStockService pharmacyStockService, ILogger<PharmacyStockController> logger)
        {
            _pharmacyStockService = pharmacyStockService;
            _logger = logger;
        }

        [HttpGet("InventoryStatusByID")]
        public IActionResult GetPharmacyInventoryStatus(int? pharmacyId)
        {

            _logger.LogInformation("GetPharmacyInventoryStatus endpoint called for pharmacyId: {PharmacyId}", pharmacyId);
            var result = _pharmacyStockService.GetPharmacyInventoryStatus(User, pharmacyId);

            if (!result.Success)
            {
                return HandleServiceError(result);
            }

            return Ok(new
            {
                success = true,
                data = result.Data,
                message = "Pharmacy inventory status retrieved successfully."
            });
        }


        [HttpGet("AllInventory")]
        public IActionResult GetAllPharmacyInventory(int? pharmacyId)
        {

            _logger.LogInformation("GetPharmacyInventory endpoint called for pharmacyId: {PharmacyId}", pharmacyId);
            var result = _pharmacyStockService.GetAllPharmacyStockInventory(User, pharmacyId);

            if (!result.Success)
            {
                return HandleServiceError(result);
            }

            return Ok(new
            {
                success = true,
                data = result.Data,
                message = "Pharmacy inventory  retrieved successfully."
            });
        }




        [HttpGet("GetBatchOfPharmacyStock")]
        public IActionResult GetPharmacyStockByPharmacyId(int pharmacyId, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("GetPharmacyStock endpoint called with pharmacyId: {PharmacyId}, pageNumber: {PageNumber}, pageSize: {PageSize}", 
                    pharmacyId, pageNumber, pageSize);


                var result = _pharmacyStockService.GetPharmacyStockByPharmacyID(pharmacyId, pageNumber, pageSize);

                if (!result.Success)
                {
                    return HandleServiceError(result);
                }

                return Ok(new
                {
                    success = true,
                    data = result.Data,
                    message = "Pharmacy stock retrieved successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in GetPharmacyStock endpoint");
                return StatusCode(500, new 
                { 
                    success = false,
                    message = "An internal server error occurred.",
                    details = ex.Message
                });
            }
        }
        [HttpGet("allPharmaciesStock")]
        public IActionResult GetAllPharmaciesStock(int pageNumber , int pageSize) 
        {
            try
            {
                _logger.LogInformation("GetPharmacyStock endpoint called with pageNumber: {PageNumber}, pageSize: {PageSize}", 
                    pageNumber, pageSize);
                var result = _pharmacyStockService.GetPharmacyStock(pageNumber, pageSize);
                if (!result.Success)
                {
                    return HandleServiceError(result);
                }
                return Ok(new
                {
                    success = true,
                    data = result.Data,
                    message = "All pharmacy stock retrieved successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in GetPharmacyStock endpoint");
                return StatusCode(500, new 
                { 
                    success = false,
                    message = "An internal server error occurred.",
                    details = ex.Message
                });
            }

        }

        [HttpGet("{category}")]
        public IActionResult GetPharmacyStockByCategory(int pharmacyId, string category, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("GetPharmacyStockByCategory endpoint called with category: {Category},pharmacyID , pageNumber: {PageNumber}, pageSize: {PageSize}",
                    category, pageNumber, pageSize);
                var result = _pharmacyStockService.GetPharmacyStockByCategory(pharmacyId, category, pageNumber, pageSize);
                if (!result.Success)
                {
                    return HandleServiceError(result);
                }
                return Ok(new
                {
                    success = true,
                    data = result.Data,
                    message = "Pharmacy stock by category retrieved successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in GetPharmacyStockByCategory endpoint");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An internal server error occurred.",
                    details = ex.Message
                });
            }
        }

        [HttpGet("DrugName/")]
        public IActionResult GetPharmacyStockByDrugName(int pharmacyId, string drugName, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("GetPharmacyStockByDrugName endpoint called with Drug Name: {drugName},pharmacyID , pageNumber: {PageNumber}, pageSize: {PageSize}",
                    drugName, pageNumber, pageSize);
                var result = _pharmacyStockService.GetPharmacyStockByDrugName(pharmacyId, drugName, pageNumber, pageSize);
                if (!result.Success)
                {
                    return HandleServiceError(result);
                }
                return Ok(new
                {
                    success = true,
                    data = result.Data,
                    message = "Pharmacy stock by Drug Name retrieved successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in GetPharmacyStockByDrugName endpoint");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An internal server error occurred.",
                    details = ex.Message
                });
            }
        }



        [HttpGet("ActiveIngrediante/")]
        public IActionResult GetPharmacyStockByActiveIngrediante(int pharmacyId, string activeIngrediante, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("GetPharmacyStockByDrugName endpoint called with Active Ingrediante : {activeIngrediante},pharmacyID , pageNumber: {PageNumber}, pageSize: {PageSize}",
                    activeIngrediante, pageNumber, pageSize);
                var result = _pharmacyStockService.GetPharmacyStockByActiveIngrediante(pharmacyId, activeIngrediante , pageNumber, pageSize);
                if (!result.Success)
                {
                    return HandleServiceError(result);
                }
                return Ok(new
                {
                    success = true,
                    data = result.Data,
                    message = "Pharmacy stock by active Ingrediante  retrieved successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in GetPharmacyStockByActiveIngrediante endpoint");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An internal server error occurred.",
                    details = ex.Message
                });
            }
        }

        [HttpGet("SearchFor")]
        public IActionResult SearchByNameOrCategoryOrActiveingrediante([FromQuery] int? pharmacyId,[FromQuery] string q,[FromQuery] int pageNumber = 1,[FromQuery] int pageSize = 10)
        {
            var result = _pharmacyStockService.SearchByNameOrCategoryOrActiveingrediante(User,pharmacyId, q, pageNumber, pageSize);
            if (!result.Success)
            {
                return Ok(new
                {
                    success = true,
                    data = new List<PharmacyProductDetailsDTO> (),
                    message = "Pharmacy stock retrieved successfully : No Search Result"
                });
            }
            return Ok(new
            {
                success = true,
                data = result.Data,
                message = "Pharmacy stock Search end point retrieved successfully."
            });
        }




        [HttpGet("{pharmacyId}/{drugId}")]
        public IActionResult GetPharmacyProductDetails(int pharmacyId, int drugId)
        {
            try
            {
                _logger.LogInformation("GetPharmacyProductDetails endpoint called for pharmacyId: {PharmacyId}, drugId: {DrugId}", pharmacyId, drugId);

                var result = _pharmacyStockService.GetPharmacyProductDetails(pharmacyId, drugId);

                if (!result.Success)
                {
                    return HandleServiceError(result);
                }

                return Ok(new
                {
                    success = true,
                    data = result.Data,
                    message = "Pharmacy product details retrieved successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in GetPharmacyProductDetails endpoint");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An internal server error occurred.",
                    details = ex.Message
                });
            }
        }

        [HttpGet("{drugId}/pharmacies")]
        //[Authorize(Policy = "PatientOnly")]
        public IActionResult GetPharmaciesThatHaveDrug(int drugId)
        {
            try
            {
                _logger.LogInformation("GetPharmaciesThatHaveDrug endpoint called for drugId: {DrugId}", drugId);
                var result = _pharmacyStockService.getPharmaciesThatHaveDrug(drugId);
                if (!result.Success)
                {
                    return HandleServiceError(result);
                }
                return Ok(new
                {
                    success = true,
                    data = result.Data,
                    message = "Pharmacies that have the drug retrieved successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in GetPharmaciesThatHaveDrug endpoint for drugId: {DrugId}", drugId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An internal server error occurred.",
                    details = ex.Message
                });
            }
        }

        [HttpPost]
        [Authorize(Policy = "PharmacyAdmin")]
        public IActionResult AddProductsToPharmacyStock([FromBody] PharmacyStockDTO pharmacyStockDTO, int? pharmacyId = null)
        {
            try
            {
                _logger.LogInformation("AddProductsToPharmacyStock endpoint called with {ProductCount} products", 
                    pharmacyStockDTO?.Products?.Count ?? 0);

                var result = _pharmacyStockService.AddProductsToPharmacyStock(User, pharmacyStockDTO, pharmacyId);

                if (!result.Success)
                {
                    return HandleServiceError(result);
                }

                return StatusCode(201, new
                {
                    success = true,
                    message = "Products added to pharmacy stock successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in AddProductsToPharmacyStock endpoint");
                return StatusCode(500, new 
                { 
                    success = false,
                    message = "An internal server error occurred.",
                    details = ex.Message
                });
            }
        }

        [HttpPut()]
        [Authorize(Policy = "PharmacyAdmin")]
        public IActionResult UpdatePharmacyProduct([FromBody] pharmacyProductDTO pharmacyProductDTO, int? pharmacyId = null)
        {
            try
            {
                _logger.LogInformation("UpdatePharmacyProduct endpoint called for drugId: {DrugId}", pharmacyProductDTO?.DrugId);

                var result = _pharmacyStockService.UpdatePharmacyProduct(User, pharmacyProductDTO, pharmacyId);

                if (!result.Success)
                {
                    return HandleServiceError(result);
                }

                return Ok(new
                {
                    success = true,
                    message = "Pharmacy product updated successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in UpdatePharmacyProduct endpoint for drugId: {DrugId}", pharmacyProductDTO?.DrugId);
                return StatusCode(500, new 
                { 
                    success = false,
                    message = "An internal server error occurred.",
                    details = ex.Message
                });
            }
        }

        [HttpDelete("{drugId}")]
        [Authorize(Policy = "PharmacyAdmin")]
        public IActionResult DeletePharmacyProduct(int drugId, int? pharmacyId = null)
        {
            try
            {
                _logger.LogInformation("DeletePharmacyProduct endpoint called for drugId: {DrugId}", drugId);

                var result = _pharmacyStockService.DeletePharmacyProduct(User, drugId, pharmacyId);

                if (!result.Success)
                {
                    return HandleServiceError(result);
                }

                return Ok(new
                {
                    success = true,
                    message = "Pharmacy product deleted successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in DeletePharmacyProduct endpoint for drugId: {DrugId}", drugId);
                return StatusCode(500, new 
                { 
                    success = false,
                    message = "An internal server error occurred.",
                    details = ex.Message
                });
            }
        }

       
        [HttpPatch("{drugId}/increase-quantity")]
        [Authorize(Policy = "PharmacyAdmin")]
        public IActionResult IncreasePharmacyProductQuantity(int drugId, [FromBody] IncreaseQuantityDTO increaseQuantityDTO, int? pharmacyId = null)
        {
            try
            {
                _logger.LogInformation("IncreasePharmacyProductQuantity endpoint called for drugId: {DrugId}", drugId);

                var quantity = increaseQuantityDTO?.Quantity ?? 0;
                var result = _pharmacyStockService.IncreasePharmacyProductQuantity(User, drugId, quantity, pharmacyId);

                if (!result.Success)
                {
                    return HandleServiceError(result);
                }

                return Ok(new
                {
                    success = true,
                    message = "Pharmacy product quantity increased successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in IncreasePharmacyProductQuantity endpoint for drugId: {DrugId}", drugId);
                return StatusCode(500, new 
                { 
                    success = false,
                    message = "An internal server error occurred.",
                    details = ex.Message
                });
            }
        }

        [HttpPatch("{drugId}/decrease-quantity")]
        [Authorize(Policy = "PharmacyAdmin")]
        public IActionResult DecreasePharmacyProductQuantity(int drugId, [FromBody] DecreaseQuantityDTO decreaseQuantityDTO, int? pharmacyId = null)
        {
            try
            {
                _logger.LogInformation("DecreasePharmacyProductQuantity endpoint called for drugId: {DrugId}", drugId);

                var quantity = decreaseQuantityDTO?.Quantity ?? 0;
                var result = _pharmacyStockService.DecreasePharmacyProductQuantity(User, drugId, quantity, pharmacyId);

                if (!result.Success)
                {
                    return HandleServiceError(result);
                }

                return Ok(new
                {
                    success = true,
                    message = "Pharmacy product quantity decreased successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in DecreasePharmacyProductQuantity endpoint for drugId: {DrugId}", drugId);
                return StatusCode(500, new 
                { 
                    success = false,
                    message = "An internal server error occurred.",
                    details = ex.Message
                });
            }
        }

      

        [HttpPatch("{drugId}/price")]
        [Authorize(Policy = "PharmacyAdmin")]
        public IActionResult UpdatePharmacyProductPrice(int drugId, [FromBody] UpdatePriceOnlyDTO updatePriceDTO, int? pharmacyId = null)
        {
            try
            {
                _logger.LogInformation("UpdatePharmacyProductPrice endpoint called for drugId: {DrugId}", drugId);

                var price = updatePriceDTO?.Price ?? 0;
                var result = _pharmacyStockService.UpdatePharmacyProductPrice(User, drugId, price, pharmacyId);

                if (!result.Success)
                {
                    return HandleServiceError(result);
                }

                return Ok(new
                {
                    success = true,
                    message = "Pharmacy product price updated successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in UpdatePharmacyProductPrice endpoint for drugId: {DrugId}", drugId);
                return StatusCode(500, new 
                { 
                    success = false,
                    message = "An internal server error occurred.",
                    details = ex.Message
                });
            }
        }


        private IActionResult HandleServiceError<T>(ServiceResult<T> result)
        {
            var response = new
            {
                success = false,
                message = result.ErrorMessage,
                errors = result.ValidationErrors.Any() ? result.ValidationErrors : null
            };

            return result.ErrorType switch
            {
                ErrorType.Validation => BadRequest(response),
                ErrorType.NotFound => NotFound(response),
                ErrorType.Conflict => Conflict(response),
                ErrorType.BusinessRule => BadRequest(response),
                ErrorType.Database => StatusCode(503, response), // Service Unavailable
                ErrorType.Authorization => Forbid(),
                ErrorType.Internal or _ => StatusCode(500, response)
            };
        }

    }
}
