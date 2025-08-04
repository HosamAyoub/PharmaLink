using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using PharmaLink_API.Core.Constants;
using PharmaLink_API.Core.Enums;
using PharmaLink_API.Core.Extensions;
using PharmaLink_API.Core.Results;
using PharmaLink_API.Models;
using PharmaLink_API.Models.DTO.PharmacyStockDTO;
using PharmaLink_API.Repository;
using PharmaLink_API.Repository.Interfaces;
using PharmaLink_API.Services.Interfaces;
using System.Security.Claims;

namespace PharmaLink_API.Services
{
    /// <summary>
    /// Implementation of pharmacy stock management service using Entity Framework, AutoMapper, 
    /// FluentValidation, and comprehensive logging for all operations.
    /// </summary>
    public class PharmacyStockService : IPharmacyStockService
    {
        private readonly IPharmacyStockRepository _pharmacyStockRepository;
        private readonly IMapper _mapper;
        private readonly IValidator<pharmacyProductDTO> _validator;
        private readonly IValidator<PharmacyStockDTO> _pharmacyStockDTOValidator;
        private readonly IValidator<UpdatePriceOnlyDTO> _updatePriceOnlyDTOValidator;
        private readonly IValidator<IncreaseQuantityDTO> _increaseQuantityDTOValidator;
        private readonly IValidator<DecreaseQuantityDTO> _decreaseQuantityDTOValidator;
        private readonly ILogger<PharmacyStockService> _logger;

        /// <summary>
        /// Initializes the service with required dependencies for validation, mapping, and data access.
        /// </summary>
        public PharmacyStockService(
            IPharmacyStockRepository pharmacyStockRepository,
            IMapper mapper,
            IValidator<pharmacyProductDTO> validator,
            IValidator<PharmacyStockDTO> pharmacyStockDTOValidator,
            IValidator<UpdatePriceOnlyDTO> updatePriceOnlyDTOValidator,
            IValidator<IncreaseQuantityDTO> increaseQuantityDTOValidator,
            IValidator<DecreaseQuantityDTO> decreaseQuantityDTOValidator,
            ILogger<PharmacyStockService> logger)
        {
            _pharmacyStockRepository = pharmacyStockRepository;
            _mapper = mapper;
            _validator = validator;
            _pharmacyStockDTOValidator = pharmacyStockDTOValidator;
            _updatePriceOnlyDTOValidator = updatePriceOnlyDTOValidator;
            _increaseQuantityDTOValidator = increaseQuantityDTOValidator;
            _decreaseQuantityDTOValidator = decreaseQuantityDTOValidator;
            _logger = logger;
        }



        public ServiceResult<PharmaInventoryDTO> GetPharmacyInventoryStatus(int pharmacyId)
        {
            try
            {
                _logger.LogInformation("Getting pharmacy inventory status for pharmacyId {PharmacyId}", pharmacyId);
                // Input validation
                if (pharmacyId <= 0)
                {
                    return ServiceResult<PharmaInventoryDTO>.ErrorResult("Pharmacy ID must be a positive number.", ErrorType.Validation);
                }
                var pharmacyStock = _pharmacyStockRepository.GetAllPharmacyStockByPharmacyID(pharmacyId);

                if (pharmacyStock == null)
                {
                    return ServiceResult<PharmaInventoryDTO>.ErrorResult(
                        $"No inventory found for pharmacy ID {pharmacyId}.",
                        ErrorType.NotFound);
                }

                return ServiceResult<PharmaInventoryDTO>.SuccessResult( new PharmaInventoryDTO
                {
                    InStockCount = pharmacyStock.Count(stock => stock.QuantityAvailable > 0),
                    OutOfStockCount = pharmacyStock.Count(stock => stock.QuantityAvailable == 0),
                    TotalCount = pharmacyStock.Count(),
                    LowStockCount = pharmacyStock.Count(stock => stock.QuantityAvailable > 0 && stock.QuantityAvailable < 12)
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Invalid operation while getting pharmacy inventory status");
                return ServiceResult<PharmaInventoryDTO>.ErrorResult(
                    "Failed to retrieve pharmacy inventory status.",
                    ErrorType.Database);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while getting pharmacy inventory status");
                return ServiceResult<PharmaInventoryDTO>.ErrorResult(
                    "An unexpected error occurred while retrieving pharmacy inventory status.",
                    ErrorType.Internal);
            }
        }

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">Thrown when database operation fails</exception>
        /// <exception cref="Exception">Thrown for unexpected errors</exception>
        public ServiceResult<PharmacyStockDTO_WithPagination> GetPharmacyStockByPharmacyID(int pharmacyId, int pageNumber, int pageSize)
        {
            try
            {
                _logger.LogInformation("Getting pharmacy stock for pharmacyId {PharmacyId}",
                     pharmacyId);
              
                // Input validation
                if (pharmacyId <= 0)
                {
                    return ServiceResult<PharmacyStockDTO_WithPagination>.ErrorResult("Pharmacy ID must be a positive number.", ErrorType.Validation);
                }

                if (pageNumber < 0 || pageSize < 0)
                {
                    return ServiceResult<PharmacyStockDTO_WithPagination>.ErrorResult(
                        "Page number and page size must be non-negative.",
                        ErrorType.Validation);
                }

                if (pageSize > 100)
                {
                    return ServiceResult<PharmacyStockDTO_WithPagination>.ErrorResult(
                        "Page size cannot exceed 100.",
                        ErrorType.Validation);
                }

                    var pharmacyStockCount = _pharmacyStockRepository.getPharmacyStockCount(pharmacyId);

                    var pharmacyStock = _pharmacyStockRepository.GetPharmacyStockByPharmacyID(pharmacyId, pageNumber, pageSize).ToList();

                if (!pharmacyStock.Any())
                {
                    return ServiceResult<PharmacyStockDTO_WithPagination>.ErrorResult(
                        "No pharmacy stock found for this pharmacy.",
                        ErrorType.NotFound);
                }

                var pharmacyStockDetailsDTOs = pharmacyStock.Select(stock => new PharmacyProductDetailsDTO
                {
                    DrugId = stock.DrugId,
                    DrugName = stock.Drug?.CommonName,
                    DrugActiveIngredient = stock.Drug?.ActiveIngredient,
                    DrugCategory = stock.Drug?.Category,
                    DrugDescription = stock.Drug?.Description,
                    DrugImageUrl = stock.Drug?.Drug_UrlImg,
                    PharmacyId = stock.PharmacyId,
                    PharmacyName = stock.Pharmacy?.Name,
                    Price = stock.Price,
                    QuantityAvailable = stock.QuantityAvailable
                }).ToList();

                var pharmacyStockPagination = new PharmacyStockDTO_WithPagination()
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalItems = pharmacyStockCount,
                    Items = pharmacyStockDetailsDTOs

                };

                _logger.LogInformation("Successfully retrieved {Count} pharmacy stock items", pharmacyStockDetailsDTOs.Count);
                return ServiceResult<PharmacyStockDTO_WithPagination>.SuccessResult(pharmacyStockPagination);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Invalid operation while getting pharmacy stock");
                return ServiceResult<PharmacyStockDTO_WithPagination>.ErrorResult(
                    "Failed to retrieve pharmacy stock.",
                    ErrorType.Database);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while getting pharmacy stock");
                return ServiceResult<PharmacyStockDTO_WithPagination>.ErrorResult(
                    "An unexpected error occurred while retrieving pharmacy stock.",
                    ErrorType.Internal);
            }
        }

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">Thrown when database operation fails</exception>
        /// <exception cref="Exception">Thrown for unexpected errors</exception>
        public ServiceResult<List<PharmacyProductDetailsDTO>> GetPharmacyStock(int pageNumber, int pageSize)
        {
            try
            {
                _logger.LogInformation("Getting all pharmacy stock with pageNumber: {PageNumber}, pageSize: {PageSize}", pageNumber, pageSize);
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
                var pharmacyStock = _pharmacyStockRepository.GetPharmacyStock(pageNumber, pageSize).ToList();
                if (!pharmacyStock.Any())
                {
                    return ServiceResult<List<PharmacyProductDetailsDTO>>.ErrorResult(
                        "No pharmacy stock found.",
                        ErrorType.NotFound);
                }
                var pharmacyStockDetailsDTOs = pharmacyStock.Select(stock => new PharmacyProductDetailsDTO
                {
                    DrugId = stock.DrugId,
                    DrugName = stock.Drug?.CommonName,
                    DrugActiveIngredient = stock.Drug?.ActiveIngredient,
                    DrugCategory = stock.Drug?.Category,
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

        /// <inheritdoc />
        /// <remarks>Uses FluentValidation for DTO validation and AutoMapper for object mapping.</remarks>
        /// <exception cref="ArgumentException">Thrown when validation fails</exception>
        /// <exception cref="InvalidOperationException">Thrown when business rules are violated</exception>
        /// <exception cref="Exception">Thrown for unexpected errors</exception>
        public ServiceResult<bool> AddProductsToPharmacyStock(ClaimsPrincipal user, PharmacyStockDTO pharmacyStockDTO, int? pharmacyId)
        {
            try
            {
                _logger.LogInformation("Adding products to pharmacy stock for user {UserId}", user.Identity?.Name);

                // FluentValidation for the DTO
                if (pharmacyStockDTO == null)
                {
                    return ServiceResult<bool>.ErrorResult("Pharmacy stock data cannot be null.", ErrorType.Validation);
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

        /// <inheritdoc />
        /// <remarks>Validates existence of product before updating and uses AutoMapper for object mapping.</remarks>
        /// <exception cref="ArgumentException">Thrown when validation fails</exception>
        /// <exception cref="InvalidOperationException">Thrown when product not found or business rules violated</exception>
        /// <exception cref="Exception">Thrown for unexpected errors</exception>
        public ServiceResult<bool> UpdatePharmacyProduct(ClaimsPrincipal user, pharmacyProductDTO pharmacyProductDTO, int? pharmacyId)
        {
            try
            {
                _logger.LogInformation("Updating pharmacy product for user {UserId}", user.Identity?.Name);

                // FluentValidation for the DTO
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

        /// <inheritdoc />
        /// <remarks>Checks for product existence before deletion and handles business rule violations for active orders/carts.</remarks>
        /// <exception cref="InvalidOperationException">Thrown when product has active orders or in customer carts</exception>
        /// <exception cref="Exception">Thrown for unexpected errors</exception>
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

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">Thrown when database operation fails</exception>
        /// <exception cref="Exception">Thrown for unexpected errors</exception>
        public ServiceResult<PharmacyStockDTO_WithPagination> GetPharmacyStockByCategory(int pharmacyId, string category, int pageNumber, int pageSize)
        {
            try
            {
                _logger.LogInformation("Getting pharmacy stock by category {Category} for pharmacyId {pharmacyId}", category, pharmacyId);
                // Input validation
                if (string.IsNullOrWhiteSpace(category))
                {
                    return ServiceResult<PharmacyStockDTO_WithPagination>.ErrorResult("Category cannot be null or empty.", ErrorType.Validation);
                }
                if (pageNumber < 0 || pageSize < 0)
                {
                    return ServiceResult<PharmacyStockDTO_WithPagination>.ErrorResult(
                        "Page number and page size must be non-negative.",
                        ErrorType.Validation);
                }
                if (pageSize > 100)
                {
                    return ServiceResult<PharmacyStockDTO_WithPagination>.ErrorResult(
                        "Page size cannot exceed 100.",
                        ErrorType.Validation);
                }

                var pharmacyStockCount = _pharmacyStockRepository.getPharmacyStockCountByCategory(pharmacyId, category);

                var pharmacyStock = _pharmacyStockRepository.getPharmacyStockByCategory(pharmacyId, category, pageNumber, pageSize).ToList();
                if (!pharmacyStock.Any())
                {
                    return ServiceResult<PharmacyStockDTO_WithPagination>.ErrorResult(
                        $"No products found in category '{category}' for this pharmacy.",
                        ErrorType.NotFound);
                }
                var pharmacyStockDetailsDTOs = pharmacyStock.Select(stock => new PharmacyProductDetailsDTO
                {
                    DrugId = stock.DrugId,
                    DrugName = stock.Drug?.CommonName,
                    DrugActiveIngredient = stock.Drug?.ActiveIngredient,
                    DrugCategory = stock.Drug?.Category,
                    DrugDescription = stock.Drug?.Description,
                    DrugImageUrl = stock.Drug?.Drug_UrlImg,
                    PharmacyId = stock.PharmacyId,
                    PharmacyName = stock.Pharmacy?.Name,
                    Price = stock.Price,
                    QuantityAvailable = stock.QuantityAvailable
                }).ToList();
                _logger.LogInformation("Successfully retrieved {Count} products in category {Category}",
                    pharmacyStockDetailsDTOs.Count, category);

                var pharmacyStockPagination = new PharmacyStockDTO_WithPagination() 
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalItems = pharmacyStockCount,
                    Items = pharmacyStockDetailsDTOs
                };


                return ServiceResult<PharmacyStockDTO_WithPagination>.SuccessResult(pharmacyStockPagination);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Invalid operation while getting pharmacy stock by category");
                return ServiceResult<PharmacyStockDTO_WithPagination>.ErrorResult(
                    "Failed to retrieve pharmacy stock by category.",
                    ErrorType.Database);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while getting pharmacy stock by category");
                return ServiceResult<PharmacyStockDTO_WithPagination>.ErrorResult(
                    "An unexpected error occurred while retrieving pharmacy stock by category.",
                    ErrorType.Internal);
            }
        }



        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">Thrown when database operation fails</exception>
        /// <exception cref="Exception">Thrown for unexpected errors</exception>
        public ServiceResult<List<PharmacyProductDetailsDTO>> GetPharmacyStockByDrugName(int pharmacyId, string drugName, int pageNumber, int pageSize)
        {
            try
            {
                _logger.LogInformation("Getting pharmacy stock by drug name {drugName} for pharmacyId {pharmacyId}", drugName, pharmacyId);
                // Input validation
                if (string.IsNullOrWhiteSpace(drugName))
                {
                    return ServiceResult<List<PharmacyProductDetailsDTO>>.ErrorResult("Category cannot be null or empty.", ErrorType.Validation);
                }
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

                var pharmacyStock = _pharmacyStockRepository.getPharmacyStockByDrugName(pharmacyId, drugName, pageNumber, pageSize).ToList();
                if (!pharmacyStock.Any())
                {
                    return ServiceResult<List<PharmacyProductDetailsDTO>>.ErrorResult(
                        $"No products found in Drug Name '{drugName}' for the given pharmacy ID.",
                        ErrorType.NotFound);
                }
                var pharmacyStockDetailsDTOs = pharmacyStock.Select(stock => new PharmacyProductDetailsDTO
                {
                    DrugId = stock.DrugId,
                    DrugName = stock.Drug?.CommonName,
                    DrugActiveIngredient = stock.Drug?.ActiveIngredient,
                    DrugCategory = stock.Drug?.Category,
                    DrugDescription = stock.Drug?.Description,
                    DrugImageUrl = stock.Drug?.Drug_UrlImg,
                    PharmacyId = stock.PharmacyId,
                    PharmacyName = stock.Pharmacy?.Name,
                    Price = stock.Price,
                    QuantityAvailable = stock.QuantityAvailable
                }).ToList();
                _logger.LogInformation("Successfully retrieved {Count} products in drug Name {drugName}",
                    pharmacyStockDetailsDTOs.Count, drugName);

                return ServiceResult<List<PharmacyProductDetailsDTO>>.SuccessResult(pharmacyStockDetailsDTOs);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Invalid operation while getting pharmacy stock by drugName");
                return ServiceResult<List<PharmacyProductDetailsDTO>>.ErrorResult(
                    "Failed to retrieve pharmacy stock by drugName.",
                    ErrorType.Database);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while getting pharmacy stock by drugName");
                return ServiceResult<List<PharmacyProductDetailsDTO>>.ErrorResult(
                    "An unexpected error occurred while retrieving pharmacy stock by drugName.",
                    ErrorType.Internal);
            }
        }


        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">Thrown when database operation fails</exception>
        /// <exception cref="Exception">Thrown for unexpected errors</exception>
        public ServiceResult<List<PharmacyProductDetailsDTO>> GetPharmacyStockByActiveIngrediante(int pharmacyId, string activeIngrediante, int pageNumber, int pageSize)
        {
            try
            {
                _logger.LogInformation("Getting pharmacy stock by drug name {activeIngrediante} for pharmacyId {pharmacyId}", activeIngrediante, pharmacyId);
                // Input validation
                if (string.IsNullOrWhiteSpace(activeIngrediante))
                {
                    return ServiceResult<List<PharmacyProductDetailsDTO>>.ErrorResult("activeIngrediante cannot be null or empty.", ErrorType.Validation);
                }
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

                var pharmacyStock = _pharmacyStockRepository.getPharmacyStockByActiveIngrediante(pharmacyId, activeIngrediante, pageNumber, pageSize).ToList();
                if (!pharmacyStock.Any())
                {
                    return ServiceResult<List<PharmacyProductDetailsDTO>>.ErrorResult(
                        $"No products found in active Ingrediante Name '{activeIngrediante}' for the given pharmacy ID.",
                        ErrorType.NotFound);
                }
                var pharmacyStockDetailsDTOs = pharmacyStock.Select(stock => new PharmacyProductDetailsDTO
                {
                    DrugId = stock.DrugId,
                    DrugName = stock.Drug?.CommonName,
                    DrugActiveIngredient = stock.Drug?.ActiveIngredient,
                    DrugCategory = stock.Drug?.Category,
                    DrugDescription = stock.Drug?.Description,
                    DrugImageUrl = stock.Drug?.Drug_UrlImg,
                    PharmacyId = stock.PharmacyId,
                    PharmacyName = stock.Pharmacy?.Name,
                    Price = stock.Price,
                    QuantityAvailable = stock.QuantityAvailable
                }).ToList();
                _logger.LogInformation("Successfully retrieved {Count} products in activeIngrediante Name {activeIngrediante}",
                    pharmacyStockDetailsDTOs.Count, activeIngrediante);

                return ServiceResult<List<PharmacyProductDetailsDTO>>.SuccessResult(pharmacyStockDetailsDTOs);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Invalid operation while getting pharmacy stock by activeIngrediante");
                return ServiceResult<List<PharmacyProductDetailsDTO>>.ErrorResult(
                    "Failed to retrieve pharmacy stock by activeIngrediante.",
                    ErrorType.Database);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while getting pharmacy stock by activeIngrediante");
                return ServiceResult<List<PharmacyProductDetailsDTO>>.ErrorResult(
                    "An unexpected error occurred while retrieving pharmacy stock by activeIngrediante.",
                    ErrorType.Internal);
            }
        }

        public ServiceResult<List<PharmacyProductDetailsDTO>> SearchByNameOrCategoryOrActiveingrediante(int pharmacyID, string q, int pageNumber, int pageSize)
        {
            try
            {
                _logger.LogInformation("Searching pharmacy stock by query {Query} for pharmacyId {PharmacyId}", q, pharmacyID);
                // Input validation
                if (string.IsNullOrWhiteSpace(q))
                {
                    return ServiceResult<List<PharmacyProductDetailsDTO>>.ErrorResult("Search query cannot be null or empty.", ErrorType.Validation);
                }
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
                var Add = _pharmacyStockRepository.getPharmacyStockByDrugName(pharmacyID, q, pageNumber, pageSize).ToList();
                var SearchList = Add.UnionBy(_pharmacyStockRepository.getPharmacyStockByCategory(pharmacyID, q, pageNumber, pageSize), u => u.DrugId)
                                    .UnionBy(_pharmacyStockRepository.getPharmacyStockByActiveIngrediante(pharmacyID, q, pageNumber, pageSize), u => u.DrugId)
                                    .ToList();
                if (!SearchList.Any())
                {
                    return ServiceResult<List<PharmacyProductDetailsDTO>>.ErrorResult(
                        $"No products found for the search query '{q}' in pharmacy ID {pharmacyID}.",
                        ErrorType.NotFound);
                }
                var pharmacyStockDetailsDTOs = SearchList.Select(stock => new PharmacyProductDetailsDTO
                {
                    DrugId = stock.DrugId,
                    DrugName = stock.Drug?.CommonName,
                    DrugActiveIngredient = stock.Drug?.ActiveIngredient,
                    DrugCategory = stock.Drug?.Category,
                    DrugDescription = stock.Drug?.Description,
                    DrugImageUrl = stock.Drug?.Drug_UrlImg,
                    PharmacyId = stock.PharmacyId,
                    PharmacyName = stock.Pharmacy?.Name,
                    Price = stock.Price,
                    QuantityAvailable = stock.QuantityAvailable
                }).ToList();
                _logger.LogInformation("Successfully retrieved {Count} products for search query {Query}",
                    pharmacyStockDetailsDTOs.Count, q);
                return ServiceResult<List<PharmacyProductDetailsDTO>>.SuccessResult(pharmacyStockDetailsDTOs);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Invalid operation while searching pharmacy stock");
                return ServiceResult<List<PharmacyProductDetailsDTO>>.ErrorResult(
                    "Failed to search pharmacy stock.",
                    ErrorType.Database);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while searching pharmacy stock");
                return ServiceResult<List<PharmacyProductDetailsDTO>>.ErrorResult(
                    "An unexpected error occurred while retrieving pharmacy stock.",
                    ErrorType.Internal);
            }
        }


        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">Thrown when database operation fails</exception>
        /// <exception cref="Exception">Thrown for unexpected errors</exception>
        public ServiceResult<PharmacyProductDetailsDTO> GetPharmacyProductDetails(int pharmacyId, int drugId)
        {
            try
            {
                _logger.LogInformation("Getting pharmacy product details for pharmacyId {PharmacyId} and drugId {DrugId}", pharmacyId, drugId);

                if (pharmacyId <= 0)
                {
                    return ServiceResult<PharmacyProductDetailsDTO>.ErrorResult("Pharmacy ID must be a positive number.", ErrorType.Validation);
                }

                if (drugId <= 0)
                {
                    return ServiceResult<PharmacyProductDetailsDTO>.ErrorResult("Drug ID must be a positive number.", ErrorType.Validation);
                }

                var product = _pharmacyStockRepository.GetPharmacyProductWithDetails(pharmacyId, drugId);
                if (product == null)
                {
                    return ServiceResult<PharmacyProductDetailsDTO>.ErrorResult(
                        $"Product with Drug ID {drugId} not found in pharmacy {pharmacyId}.",
                        ErrorType.NotFound);
                }

                var productDetailsDTO = new PharmacyProductDetailsDTO
                {
                    DrugId = product.DrugId,
                    DrugName = product.Drug?.CommonName,
                    DrugActiveIngredient = product.Drug?.ActiveIngredient,
                    DrugCategory = product.Drug?.Category,
                    DrugDescription = product.Drug?.Description,
                    DrugImageUrl = product.Drug?.Drug_UrlImg,
                    PharmacyId = product.PharmacyId,
                    PharmacyName = product.Pharmacy?.Name,
                    Price = product.Price,
                    QuantityAvailable = product.QuantityAvailable
                };

                _logger.LogInformation("Successfully retrieved pharmacy product details for pharmacyId {PharmacyId} and drugId {DrugId}", pharmacyId, drugId);
                return ServiceResult<PharmacyProductDetailsDTO>.SuccessResult(productDetailsDTO);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Invalid operation while getting pharmacy product details");
                return ServiceResult<PharmacyProductDetailsDTO>.ErrorResult(
                    "Failed to retrieve pharmacy product details.",
                    ErrorType.Database);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while getting pharmacy product details");
                return ServiceResult<PharmacyProductDetailsDTO>.ErrorResult(
                    "An unexpected error occurred while retrieving pharmacy product details.",
                    ErrorType.Internal);
            }
        }

        /// <inheritdoc />
        /// <remarks>Uses FluentValidation for price validation before updating.</remarks>
        /// <exception cref="ArgumentException">Thrown when validation fails</exception>
        /// <exception cref="InvalidOperationException">Thrown when product not found or business rules violated</exception>
        /// <exception cref="Exception">Thrown for unexpected errors</exception>
        public ServiceResult<bool> UpdatePharmacyProductPrice(ClaimsPrincipal user, int drugId, decimal newPrice, int? pharmacyId)
        {
            try
            {
                _logger.LogInformation("Updating pharmacy product price for user {UserId}", user.Identity?.Name);

                // Create DTO for validation
                var updatePriceDTO = new UpdatePriceOnlyDTO { Price = newPrice };

                // FluentValidation
                var validationResult = _updatePriceOnlyDTOValidator.Validate(updatePriceDTO);
                if (!validationResult.IsValid)
                {
                    return ServiceResult<bool>.ValidationErrorResult(validationResult.Errors.Select(e => e.ErrorMessage).ToList());
                }

                if (drugId <= 0)
                {
                    return ServiceResult<bool>.ErrorResult("Drug ID must be a positive number.", ErrorType.Validation);
                }

                var pharmacyIdResult = GetPharmacyIdForUser(user, pharmacyId);
                if (!pharmacyIdResult.Success)
                    return ServiceResult<bool>.ErrorResult(
                        pharmacyIdResult.ErrorMessage,
                        pharmacyIdResult.ErrorType ?? ErrorType.Authorization);

                var existingStock = _pharmacyStockRepository.GetPharmacyProduct(pharmacyIdResult.Data, drugId);
                if (existingStock == null)
                {
                    return ServiceResult<bool>.ErrorResult(
                        $"Product with Drug ID {drugId} not found in pharmacy {pharmacyIdResult.Data}.",
                        ErrorType.NotFound);
                }

                _pharmacyStockRepository.UpdatePharmacyProductPrice(pharmacyIdResult.Data, drugId, newPrice);

                _logger.LogInformation("Successfully updated pharmacy product price {DrugId} for pharmacy {PharmacyId} to {NewPrice}",
                    drugId, pharmacyIdResult.Data, newPrice);
                return ServiceResult<bool>.SuccessResult(true);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Validation error while updating pharmacy product price");
                return ServiceResult<bool>.ErrorResult(ex.Message, ErrorType.Validation);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Business rule violation while updating pharmacy product price");

                if (ex.Message.Contains("not found"))
                    return ServiceResult<bool>.ErrorResult(ex.Message, ErrorType.NotFound);
                else if (ex.Message.Contains("modified by another user"))
                    return ServiceResult<bool>.ErrorResult(ex.Message, ErrorType.Conflict);
                else
                    return ServiceResult<bool>.ErrorResult(ex.Message, ErrorType.Database);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating pharmacy product price");
                return ServiceResult<bool>.ErrorResult(
                    "An unexpected error occurred while updating pharmacy product price.",
                    ErrorType.Internal);
            }
        }

        /// <inheritdoc />
        /// <remarks>Uses FluentValidation for quantity validation before updating.</remarks>
        /// <exception cref="ArgumentException">Thrown when validation fails</exception>
        /// <exception cref="InvalidOperationException">Thrown when product not found or business rules violated</exception>
        /// <exception cref="Exception">Thrown for unexpected errors</exception>
        public ServiceResult<bool> IncreasePharmacyProductQuantity(ClaimsPrincipal user, int drugId, int quantityToIncrease, int? pharmacyId)
        {
            try
            {
                _logger.LogInformation("Increasing pharmacy product quantity for user {UserId}", user.Identity?.Name);

                // Create DTO for validation
                var increaseQuantityDTO = new IncreaseQuantityDTO { Quantity = quantityToIncrease };

                // FluentValidation
                var validationResult = _increaseQuantityDTOValidator.Validate(increaseQuantityDTO);
                if (!validationResult.IsValid)
                {
                    return ServiceResult<bool>.ValidationErrorResult(validationResult.Errors.Select(e => e.ErrorMessage).ToList());
                }

                if (drugId <= 0)
                {
                    return ServiceResult<bool>.ErrorResult("Drug ID must be a positive number.", ErrorType.Validation);
                }

                var pharmacyIdResult = GetPharmacyIdForUser(user, pharmacyId);
                if (!pharmacyIdResult.Success)
                    return ServiceResult<bool>.ErrorResult(
                        pharmacyIdResult.ErrorMessage,
                        pharmacyIdResult.ErrorType ?? ErrorType.Authorization);

                var existingStock = _pharmacyStockRepository.GetPharmacyProduct(pharmacyIdResult.Data, drugId);
                if (existingStock == null)
                {
                    return ServiceResult<bool>.ErrorResult(
                        $"Product with Drug ID {drugId} not found in pharmacy {pharmacyIdResult.Data}.",
                        ErrorType.NotFound);
                }

                _pharmacyStockRepository.IncreasePharmacyProductQuantity(pharmacyIdResult.Data, drugId, quantityToIncrease);

                _logger.LogInformation("Successfully increased pharmacy product quantity {DrugId} for pharmacy {PharmacyId} by {QuantityToIncrease}",
                    drugId, pharmacyIdResult.Data, quantityToIncrease);
                return ServiceResult<bool>.SuccessResult(true);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Validation error while increasing pharmacy product quantity");
                return ServiceResult<bool>.ErrorResult(ex.Message, ErrorType.Validation);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Business rule violation while increasing pharmacy product quantity");

                if (ex.Message.Contains("not found"))
                    return ServiceResult<bool>.ErrorResult(ex.Message, ErrorType.NotFound);
                else if (ex.Message.Contains("modified by another user"))
                    return ServiceResult<bool>.ErrorResult(ex.Message, ErrorType.Conflict);
                else
                    return ServiceResult<bool>.ErrorResult(ex.Message, ErrorType.Database);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while increasing pharmacy product quantity");
                return ServiceResult<bool>.ErrorResult(
                    "An unexpected error occurred while increasing pharmacy product quantity.",
                    ErrorType.Internal);
            }
        }

        /// <inheritdoc />
        /// <remarks>Uses FluentValidation for quantity validation and prevents quantity from going below zero.</remarks>
        /// <exception cref="ArgumentException">Thrown when validation fails</exception>
        /// <exception cref="InvalidOperationException">Thrown when product not found, insufficient quantity, or business rules violated</exception>
        /// <exception cref="Exception">Thrown for unexpected errors</exception>
        public ServiceResult<bool> DecreasePharmacyProductQuantity(ClaimsPrincipal user, int drugId, int quantityToDecrease, int? pharmacyId)
        {
            try
            {
                _logger.LogInformation("Decreasing pharmacy product quantity for user {UserId}", user.Identity?.Name);

                // Create DTO for validation
                var decreaseQuantityDTO = new DecreaseQuantityDTO { Quantity = quantityToDecrease };

                // FluentValidation
                var validationResult = _decreaseQuantityDTOValidator.Validate(decreaseQuantityDTO);
                if (!validationResult.IsValid)
                {
                    return ServiceResult<bool>.ValidationErrorResult(validationResult.Errors.Select(e => e.ErrorMessage).ToList());
                }

                if (drugId <= 0)
                {
                    return ServiceResult<bool>.ErrorResult("Drug ID must be a positive number.", ErrorType.Validation);
                }

                var pharmacyIdResult = GetPharmacyIdForUser(user, pharmacyId);
                if (!pharmacyIdResult.Success)
                    return ServiceResult<bool>.ErrorResult(
                        pharmacyIdResult.ErrorMessage,
                        pharmacyIdResult.ErrorType ?? ErrorType.Authorization);

                var existingStock = _pharmacyStockRepository.GetPharmacyProduct(pharmacyIdResult.Data, drugId);
                if (existingStock == null)
                {
                    return ServiceResult<bool>.ErrorResult(
                        $"Product with Drug ID {drugId} not found in pharmacy {pharmacyIdResult.Data}.",
                        ErrorType.NotFound);
                }

                _pharmacyStockRepository.DecreasePharmacyProductQuantity(pharmacyIdResult.Data, drugId, quantityToDecrease);

                _logger.LogInformation("Successfully decreased pharmacy product quantity {DrugId} for pharmacy {PharmacyId} by {QuantityToDecrease}",
                    drugId, pharmacyIdResult.Data, quantityToDecrease);
                return ServiceResult<bool>.SuccessResult(true);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Validation error while decreasing pharmacy product quantity");
                return ServiceResult<bool>.ErrorResult(ex.Message, ErrorType.Validation);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Business rule violation while decreasing pharmacy product quantity");

                if (ex.Message.Contains("not found"))
                    return ServiceResult<bool>.ErrorResult(ex.Message, ErrorType.NotFound);
                else if (ex.Message.Contains("Cannot decrease quantity"))
                    return ServiceResult<bool>.ErrorResult(ex.Message, ErrorType.BusinessRule);
                else if (ex.Message.Contains("modified by another user"))
                    return ServiceResult<bool>.ErrorResult(ex.Message, ErrorType.Conflict);
                else
                    return ServiceResult<bool>.ErrorResult(ex.Message, ErrorType.Database);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while decreasing pharmacy product quantity");
                return ServiceResult<bool>.ErrorResult(
                    "An unexpected error occurred while decreasing pharmacy product quantity.",
                    ErrorType.Internal);
            }
        }

        /// <summary>
        /// Determines the appropriate pharmacy ID based on user authorization and role using claim extensions.
        /// Admin users must provide a pharmacy ID, while pharmacy users use their associated pharmacy ID from claims.
        /// </summary>
        /// <param name="user">ClaimsPrincipal containing user authorization information</param>
        /// <param name="providedPharmacyId">Optional pharmacy ID provided by the user (required for admin users)</param>
        /// <returns>A ServiceResult containing the validated pharmacy ID if successful</returns>
        /// <exception cref="Exception">Thrown for unexpected errors during claim extraction</exception>
        private ServiceResult<int> GetPharmacyIdForUser(ClaimsPrincipal user, int? providedPharmacyId)
        {
            try
            {
                if (user == null)
                {
                    return ServiceResult<int>.ErrorResult("User information is missing.", ErrorType.Authorization);
                }

                // If admin user, they must provide pharmacy ID
                if (user.IsAdmin())
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
                // If pharmacy user, get pharmacy ID from claims
                else if (user.IsPharmacy())
                {
                    var pharmacyId = user.GetPharmacyId();
                    if (!pharmacyId.HasValue || pharmacyId.Value <= 0)
                    {
                        return ServiceResult<int>.ErrorResult("Invalid or missing pharmacy ID in user claims.", ErrorType.Authorization);
                    }
                    return ServiceResult<int>.SuccessResult(pharmacyId.Value);
                }
                else
                {
                    return ServiceResult<int>.ErrorResult("User is not authorized to access pharmacy stock.", ErrorType.Authorization);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while extracting pharmacy ID for user");
                return ServiceResult<int>.ErrorResult("Failed to determine pharmacy ID.", ErrorType.Internal);
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// Uses AutoMapper to convert Pharmacy entities to PharmacyDTO objects for client consumption.
        /// Validates drugId parameter and handles various error scenarios with appropriate error types.
        /// Provides comprehensive logging for operation tracking and debugging.
        /// Designed specifically for patient use to locate pharmacies selling specific medications.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown when database operation fails</exception>
        /// <exception cref="Exception">Thrown for unexpected errors</exception>
        public ServiceResult<List<PharmacyDTO>> getPharmaciesThatHaveDrug(int drugId)
        {
            try
            {
                _logger.LogInformation("Getting pharmacies that have drug with ID {DrugId}", drugId);

                if (drugId <= 0)
                {
                    return ServiceResult<List<PharmacyDTO>>.ErrorResult("Drug ID must be a positive number.", ErrorType.Validation);
                }

                var pharmacies = _pharmacyStockRepository.getPharmaciesThatHaveDrug(drugId);
                if (pharmacies == null || !pharmacies.Any())
                {
                    return ServiceResult<List<PharmacyDTO>>.ErrorResult(
                        $"No pharmacies found for Drug ID {drugId}.",
                        ErrorType.NotFound);
                }

                var pharmaciesDTOS = _mapper.Map<List<PharmacyDTO>>(pharmacies);

                _logger.LogInformation("Successfully retrieved {Count} pharmacies for Drug ID {DrugId}", pharmaciesDTOS.Count, drugId);
                return ServiceResult<List<PharmacyDTO>>.SuccessResult(pharmaciesDTOS);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Invalid operation while getting pharmacies that have drug");
                return ServiceResult<List<PharmacyDTO>>.ErrorResult(
                    "Failed to retrieve pharmacies that have the specified drug.",
                    ErrorType.Database);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while getting pharmacies that have drug");
                return ServiceResult<List<PharmacyDTO>>.ErrorResult(
                    "An unexpected error occurred while retrieving pharmacies that have the specified drug.",
                    ErrorType.Internal);
            }
        }
    }
}