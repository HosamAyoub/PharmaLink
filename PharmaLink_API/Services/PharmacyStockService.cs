using AutoMapper;
using FluentValidation;
using PharmaLink_API.Models;
using PharmaLink_API.Models.DTO.PharmacyStockDTO;
using PharmaLink_API.Repository.Interfaces;
using PharmaLink_API.Services.Interfaces;
using PharmaLink_API.Core.Results;
using PharmaLink_API.Core.Enums;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace PharmaLink_API.Services
{
    public class PharmacyStockService : IPharmacyStockService
    {
        private readonly IPharmacyStockRepository _pharmacyStockRepository;
        private readonly IMapper _mapper;
        private readonly IValidator<pharmacyProductDTO> _validator;
        private readonly IValidator<PharmacyStockDTO> _pharmacyStockDTOValidator;
        private readonly ILogger<PharmacyStockService> _logger;

        public PharmacyStockService(
            IPharmacyStockRepository pharmacyStockRepository,
            IMapper mapper,
            IValidator<pharmacyProductDTO> validator,
            IValidator<PharmacyStockDTO> pharmacyStockDTOValidator,
            ILogger<PharmacyStockService> logger)
        {
            _pharmacyStockRepository = pharmacyStockRepository;
            _mapper = mapper;
            _validator = validator;
            _pharmacyStockDTOValidator = pharmacyStockDTOValidator;
            _logger = logger;
        }

        public ServiceResult<List<PharmacyProductDetailsDTO>> GetPharmacyStock(ClaimsPrincipal user, int? pharmacyId, int pageNumber, int pageSize)
        {
            try
            {
                _logger.LogInformation("Getting pharmacy stock for user {UserId}, pharmacyId {PharmacyId}", 
                    user.Identity?.Name, pharmacyId);

                // Input validation
                if (pageNumber < 0 || pageSize < 0)
                {
                    return ServiceResult<List<PharmacyProductDetailsDTO>>.ErrorResult(
                        "Page number and page size must be non-negative.", 
                        ErrorType.Validation);
                }

                if (pageSize > 100)
                {
                    return ServiceResult<List<PharmacyProductDetailsDTO>>.ErrorResult(
                        "Page size cannot exceed 100.", 
                        ErrorType.Validation);
                }

                var pharmacyIdResult = GetPharmacyIdForUser(user, pharmacyId);
                if (!pharmacyIdResult.Success)
                    return ServiceResult<List<PharmacyProductDetailsDTO>>.ErrorResult(
                        pharmacyIdResult.ErrorMessage, 
                        pharmacyIdResult.ErrorType ?? ErrorType.Authorization);

                var pharmacyStock = _pharmacyStockRepository.GetPharmacyStock(pharmacyIdResult.Data, pageNumber, pageSize).ToList();

                if (!pharmacyStock.Any())
                {
                    return ServiceResult<List<PharmacyProductDetailsDTO>>.ErrorResult(
                        "No pharmacy stock found for the given pharmacy ID.", 
                        ErrorType.NotFound);
                }

                var pharmacyStockDetailsDTOs = pharmacyStock.Select(stock => new PharmacyProductDetailsDTO
                {
                    DrugId = stock.DrugId,
                    DrugName = stock.Drug?.CommonName,
                    DrugDescription = stock.Drug?.Description,
                    DrugImageUrl = stock.Drug?.Drug_UrlImg,
                    PharmacyId = stock.PharmacyId,
                    PharmacyName = stock.Pharmacy?.Name,
                    Price = stock.Price,
                    QuantityAvailable = stock.QuantityAvailable
                }).ToList();

                _logger.LogInformation("Successfully retrieved {Count} pharmacy stock items", pharmacyStockDetailsDTOs.Count);
                return ServiceResult<List<PharmacyProductDetailsDTO>>.SuccessResult(pharmacyStockDetailsDTOs);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Invalid operation while getting pharmacy stock");
                return ServiceResult<List<PharmacyProductDetailsDTO>>.ErrorResult(
                    "Failed to retrieve pharmacy stock.", 
                    ErrorType.Database);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while getting pharmacy stock");
                return ServiceResult<List<PharmacyProductDetailsDTO>>.ErrorResult(
                    "An unexpected error occurred while retrieving pharmacy stock.", 
                    ErrorType.Internal);
            }
        }

        public ServiceResult<bool> AddProductsToPharmacyStock(ClaimsPrincipal user, PharmacyStockDTO pharmacyStockDTO, int? pharmacyId)
        {
            try
            {
                _logger.LogInformation("Adding products to pharmacy stock for user {UserId}", user.Identity?.Name);

                // Input validation
                if (pharmacyStockDTO == null)
                {
                    return ServiceResult<bool>.ErrorResult("Pharmacy stock data cannot be null.", ErrorType.Validation);
                }

                if (pharmacyStockDTO.Products == null || !pharmacyStockDTO.Products.Any())
                {
                    return ServiceResult<bool>.ErrorResult("Products list cannot be null or empty.", ErrorType.Validation);
                }

                var validationResult = _pharmacyStockDTOValidator.Validate(pharmacyStockDTO);
                if (!validationResult.IsValid)
                {
                    return ServiceResult<bool>.ValidationErrorResult(validationResult.Errors.Select(e => e.ErrorMessage).ToList());
                }

                var pharmacyIdResult = GetPharmacyIdForUser(user, pharmacyId);
                if (!pharmacyIdResult.Success)
                    return ServiceResult<bool>.ErrorResult(
                        pharmacyIdResult.ErrorMessage, 
                        pharmacyIdResult.ErrorType ?? ErrorType.Authorization);

                var mappedPharmacyStock = _mapper.Map<List<PharmacyProduct>>(pharmacyStockDTO.Products);
                mappedPharmacyStock.ForEach(product => product.PharmacyId = pharmacyIdResult.Data);

                if (!mappedPharmacyStock.Any())
                {
                    return ServiceResult<bool>.ErrorResult("Invalid pharmacy stock data.", ErrorType.Validation);
                }

                _pharmacyStockRepository.AddProductsToPharmacyStock(mappedPharmacyStock);
                
                _logger.LogInformation("Successfully added {Count} products to pharmacy stock", mappedPharmacyStock.Count);
                return ServiceResult<bool>.SuccessResult(true);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Validation error while adding products to pharmacy stock");
                return ServiceResult<bool>.ErrorResult(ex.Message, ErrorType.Validation);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Business rule violation while adding products to pharmacy stock");
                return ServiceResult<bool>.ErrorResult(ex.Message, ErrorType.Conflict);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while adding products to pharmacy stock");
                return ServiceResult<bool>.ErrorResult(
                    "An unexpected error occurred while adding products to pharmacy stock.", 
                    ErrorType.Internal);
            }
        }

        public ServiceResult<bool> UpdatePharmacyProduct(ClaimsPrincipal user, pharmacyProductDTO pharmacyProductDTO, int? pharmacyId)
        {
            try
            {
                _logger.LogInformation("Updating pharmacy product for user {UserId}", user.Identity?.Name);

                // Input validation
                if (pharmacyProductDTO == null)
                {
                    return ServiceResult<bool>.ErrorResult("Pharmacy product data cannot be null.", ErrorType.Validation);
                }

                var validationResult = _validator.Validate(pharmacyProductDTO);
                if (!validationResult.IsValid)
                {
                    return ServiceResult<bool>.ValidationErrorResult(validationResult.Errors.Select(e => e.ErrorMessage).ToList());
                }

                var pharmacyIdResult = GetPharmacyIdForUser(user, pharmacyId);
                if (!pharmacyIdResult.Success)
                    return ServiceResult<bool>.ErrorResult(
                        pharmacyIdResult.ErrorMessage, 
                        pharmacyIdResult.ErrorType ?? ErrorType.Authorization);

                var existingStock = _pharmacyStockRepository.GetPharmacyProduct(pharmacyIdResult.Data, pharmacyProductDTO.DrugId);
                if (existingStock == null)
                {
                    return ServiceResult<bool>.ErrorResult(
                        $"Product with Drug ID {pharmacyProductDTO.DrugId} not found in pharmacy {pharmacyIdResult.Data}.", 
                        ErrorType.NotFound);
                }

                var pharmacyProduct = _mapper.Map<PharmacyProduct>(pharmacyProductDTO);
                pharmacyProduct.PharmacyId = pharmacyIdResult.Data;

                _pharmacyStockRepository.UpdatePharmacyProduct(pharmacyProduct);
                
                _logger.LogInformation("Successfully updated pharmacy product {DrugId} for pharmacy {PharmacyId}", 
                    pharmacyProductDTO.DrugId, pharmacyIdResult.Data);
                return ServiceResult<bool>.SuccessResult(true);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Validation error while updating pharmacy product");
                return ServiceResult<bool>.ErrorResult(ex.Message, ErrorType.Validation);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Business rule violation while updating pharmacy product");
                
                if (ex.Message.Contains("not found"))
                    return ServiceResult<bool>.ErrorResult(ex.Message, ErrorType.NotFound);
                else if (ex.Message.Contains("modified by another user"))
                    return ServiceResult<bool>.ErrorResult(ex.Message, ErrorType.Conflict);
                else
                    return ServiceResult<bool>.ErrorResult(ex.Message, ErrorType.Database);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating pharmacy product");
                return ServiceResult<bool>.ErrorResult(
                    "An unexpected error occurred while updating pharmacy product.", 
                    ErrorType.Internal);
            }
        }

        public ServiceResult<bool> DeletePharmacyProduct(ClaimsPrincipal user, int productId, int? pharmacyId)
        {
            try
            {
                _logger.LogInformation("Deleting pharmacy product {ProductId} for user {UserId}", productId, user.Identity?.Name);

                // Input validation
                if (productId <= 0)
                {
                    return ServiceResult<bool>.ErrorResult("Product ID must be a positive number.", ErrorType.Validation);
                }

                var pharmacyIdResult = GetPharmacyIdForUser(user, pharmacyId);
                if (!pharmacyIdResult.Success)
                    return ServiceResult<bool>.ErrorResult(
                        pharmacyIdResult.ErrorMessage, 
                        pharmacyIdResult.ErrorType ?? ErrorType.Authorization);

                var existingPharmacyProduct = _pharmacyStockRepository.GetPharmacyProduct(pharmacyIdResult.Data, productId);
                if (existingPharmacyProduct == null)
                {
                    return ServiceResult<bool>.ErrorResult(
                        $"Product with ID {productId} not found in pharmacy {pharmacyIdResult.Data}.", 
                        ErrorType.NotFound);
                }

                _pharmacyStockRepository.DeletePharmacyProduct(existingPharmacyProduct);
                
                _logger.LogInformation("Successfully deleted pharmacy product {ProductId} from pharmacy {PharmacyId}", 
                    productId, pharmacyIdResult.Data);
                return ServiceResult<bool>.SuccessResult(true);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Business rule violation while deleting pharmacy product");
                
                if (ex.Message.Contains("active orders") || ex.Message.Contains("customer carts"))
                    return ServiceResult<bool>.ErrorResult(ex.Message, ErrorType.Conflict);
                else
                    return ServiceResult<bool>.ErrorResult(ex.Message, ErrorType.Database);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting pharmacy product {ProductId}", productId);
                return ServiceResult<bool>.ErrorResult(
                    "An unexpected error occurred while deleting pharmacy product.", 
                    ErrorType.Internal);
            }
        }

        private ServiceResult<int> GetPharmacyIdForUser(ClaimsPrincipal user, int? providedPharmacyId)
        {
            try
            {
                if (user == null)
                {
                    return ServiceResult<int>.ErrorResult("User information is missing.", ErrorType.Authorization);
                }

                if (user.IsInRole("Admin"))
                {
                    if (!providedPharmacyId.HasValue)
                    {
                        return ServiceResult<int>.ErrorResult("Admin users must specify pharmacy ID.", ErrorType.Validation);
                    }
                    
                    if (providedPharmacyId.Value <= 0)
                    {
                        return ServiceResult<int>.ErrorResult("Pharmacy ID must be a positive number.", ErrorType.Validation);
                    }

                    return ServiceResult<int>.SuccessResult(providedPharmacyId.Value);
                }
                else
                {
                    if (!int.TryParse(user.FindFirst("pharmacy_id")?.Value, out int pharmacyId) || pharmacyId <= 0)
                    {
                        return ServiceResult<int>.ErrorResult("Invalid or missing pharmacy ID in user claims.", ErrorType.Authorization);
                    }
                    return ServiceResult<int>.SuccessResult(pharmacyId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while extracting pharmacy ID for user");
                return ServiceResult<int>.ErrorResult("Failed to determine pharmacy ID.", ErrorType.Internal);
            }
        }
    }
}