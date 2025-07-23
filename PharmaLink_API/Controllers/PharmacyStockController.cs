using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaLink_API.Models.DTO.PharmacyStockDTO;
using PharmaLink_API.Services.Interfaces;
using PharmaLink_API.Core.Results;
using PharmaLink_API.Core.Enums;

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

        [HttpGet()]

        public IActionResult GetPharmacyStock(int pharmacyId, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("GetPharmacyStock endpoint called with pharmacyId: {PharmacyId}, pageNumber: {PageNumber}, pageSize: {PageSize}", 
                    pharmacyId, pageNumber, pageSize);

                var result = _pharmacyStockService.GetPharmacyStock(pharmacyId, pageNumber, pageSize);

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

        [HttpPost]
        [Authorize(Policy = "PharmacyAdmin")]
        public IActionResult AddProductsToPharmacyStock([FromBody] PharmacyStockDTO pharmacyStockDTO, int? pharmacyId = null)
        {
            try
            {
                _logger.LogInformation("AddProductsToPharmacyStock endpoint called with {ProductCount} products", 
                    pharmacyStockDTO?.Products?.Count ?? 0);

                if (pharmacyStockDTO == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Request body cannot be null.",
                        errors = new[] { "PharmacyStock is required." }
                    });
                }

                var result = _pharmacyStockService.AddProductsToPharmacyStock(User, pharmacyStockDTO, pharmacyId);

                if (!result.Success)
                {
                    return HandleServiceError(result);
                }

                return CreatedAtAction(nameof(GetPharmacyStock), new { pharmacyId }, new
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

        [HttpPut("{drugId}")]
        [Authorize(Policy = "PharmacyAdmin")]
        public IActionResult UpdatePharmacyProduct(int drugId, [FromBody] pharmacyProductDTO pharmacyProductDTO, int? pharmacyId = null)
        {
            try
            {
                _logger.LogInformation("UpdatePharmacyProduct endpoint called for drugId: {DrugId}", drugId);

                if (pharmacyProductDTO == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Request body cannot be null.",
                        errors = new[] { "pharmacyProductDTO is required." }
                    });
                }

                // Ensure the drugId from route matches the one in the DTO
                if (pharmacyProductDTO.DrugId != drugId)
                {
                    pharmacyProductDTO.DrugId = drugId;
                }

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
                _logger.LogError(ex, "Unhandled exception in UpdatePharmacyProduct endpoint for drugId: {DrugId}", drugId);
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

        [HttpGet("category/{category}")]
        public IActionResult GetPharmacyStockByCategory(int pharamcyId , string category, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("GetPharmacyStockByCategory endpoint called with category: {Category},pharmacyID , pageNumber: {PageNumber}, pageSize: {PageSize}", 
                    category, pageNumber, pageSize);
                var result = _pharmacyStockService.GetPharmacyStockByCategory(pharamcyId,category, pageNumber, pageSize);
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
