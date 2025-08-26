using PharmaLink_API.Core.Results;
using PharmaLink_API.Models;
using PharmaLink_API.Models.DTO.PharmacyDTO;
using PharmaLink_API.Models.DTO.PharmacyStockDTO;
using System.Security.Claims;

namespace PharmaLink_API.Services.Interfaces
{
    /// <summary>
    /// Defines the contract for pharmacy stock management operations including inventory management,
    /// product operations, and authorization-based access control.
    /// </summary>
    public interface IPharmacyStockService
    {

        ServiceResult<List<PharmacyProductDetailsDTO>> GetAllPharmacyStockInventory(ClaimsPrincipal user, int? pharmacyId);
        ServiceResult<PharmaInventoryDTO> GetPharmacyInventoryStatus(ClaimsPrincipal user, int? pharmacyId);
        /// <summary>
        /// Retrieves pharmacy stock for a specific pharmacy with pagination.
        /// </summary>
        /// <param name="pharmacyId">The ID of the pharmacy to retrieve stock for</param>
        /// <param name="pageNumber">Page number for pagination (must be non-negative)</param>
        /// <param name="pageSize">Number of items per page (must be non-negative, max 100)</param>
        /// <returns>A ServiceResult containing a list of PharmacyProductDetailsDTO objects</returns>
        ServiceResult<PharmacyStockDTO_WithPagination> GetPharmacyStockByPharmacyID(int pharmacyId, int pageNumber, int pageSize);

        /// <summary>
        /// Retrieves all pharmacy stock across all pharmacies with pagination.
        /// </summary>
        /// <param name="pageNumber">Page number for pagination (must be non-negative)</param>
        /// <param name="pageSize">Number of items per page (must be non-negative, max 100)</param>
        /// <returns>A ServiceResult containing a list of PharmacyProductDetailsDTO objects</returns>
        ServiceResult<List<PharmacyProductDetailsDTO>> GetPharmacyStock(int pageNumber, int pageSize, out int distinctDrugsCount);

        /// <summary>
        /// Adds multiple products to a pharmacy's stock inventory.
        /// Requires appropriate authorization (PharmacyAdmin policy).
        /// </summary>
        /// <param name="user">ClaimsPrincipal containing user authorization information</param>
        /// <param name="pharmacyStockDTO">DTO containing the list of products to add</param>
        /// <param name="pharmacyId">Optional pharmacy ID (required for admin users, ignored for pharmacy users)</param>
        /// <returns>A ServiceResult indicating success or failure of the operation</returns>
        ServiceResult<bool> AddProductsToPharmacyStock(ClaimsPrincipal user, PharmacyStockDTO pharmacyStockDTO, int? pharmacyId);

        /// <summary>
        /// Updates an existing product in a pharmacy's stock inventory.
        /// Requires appropriate authorization (PharmacyAdmin policy).
        /// </summary>
        /// <param name="user">ClaimsPrincipal containing user authorization information</param>
        /// <param name="pharmacyProductDTO">DTO containing the updated product information</param>
        /// <param name="pharmacyId">Optional pharmacy ID (required for admin users, ignored for pharmacy users)</param>
        /// <returns>A ServiceResult indicating success or failure of the operation</returns>
        ServiceResult<bool> UpdatePharmacyProduct(ClaimsPrincipal user, pharmacyProductDTO pharmacyProductDTO, int? pharmacyId);

        /// <summary>
        /// Deletes a product from a pharmacy's stock inventory.
        /// Requires appropriate authorization (PharmacyAdmin policy).
        /// </summary>
        /// <param name="user">ClaimsPrincipal containing user authorization information</param>
        /// <param name="productId">The ID of the product (drug) to delete</param>
        /// <param name="pharmacyId">Optional pharmacy ID (required for admin users, ignored for pharmacy users)</param>
        /// <returns>A ServiceResult indicating success or failure of the operation</returns>
        ServiceResult<bool> DeletePharmacyProduct(ClaimsPrincipal user, int productId, int? pharmacyId);

        /// <summary>
        /// Retrieves pharmacy stock filtered by drug category with pagination.
        /// </summary>
        /// <param name="pharmacyID">The ID of the pharmacy to retrieve stock for</param>
        /// <param name="category">The drug category to filter by</param>
        /// <param name="pageNumber">Page number for pagination (must be non-negative)</param>
        /// <param name="pageSize">Number of items per page (must be non-negative, max 100)</param>
        /// <returns>A ServiceResult containing a list of PharmacyProductDetailsDTO objects filtered by category</returns>
        ServiceResult<PharmacyStockDTO_WithPagination> GetPharmacyStockByCategory(int pharmacyID, string category, int pageNumber, int pageSize);


        /// <summary>
        /// Retrieves pharmacy stock filtered by drug category with pagination.
        /// </summary>
        /// <param name="pharmacyID">The ID of the pharmacy to retrieve stock for</param>
        /// <param name="drugName">The drug Name to filter by</param>
        /// <param name="pageNumber">Page number for pagination (must be non-negative)</param>
        /// <param name="pageSize">Number of items per page (must be non-negative, max 100)</param>
        /// <returns>A ServiceResult containing a list of PharmacyProductDetailsDTO objects filtered by category</returns>
        ServiceResult<List<PharmacyProductDetailsDTO>> GetPharmacyStockByDrugName(int pharmacyID, string drugName, int pageNumber, int pageSize);


        /// <summary>
        /// Retrieves pharmacy stock filtered by drug category with pagination.
        /// </summary>
        /// <param name="pharmacyID">The ID of the pharmacy to retrieve stock for</param>
        /// <param name="activeIngrediante">The activeIngrediante Name to filter by</param>
        /// <param name="pageNumber">Page number for pagination (must be non-negative)</param>
        /// <param name="pageSize">Number of items per page (must be non-negative, max 100)</param>
        /// <returns>A ServiceResult containing a list of PharmacyProductDetailsDTO objects filtered by category</returns>
        ServiceResult<List<PharmacyProductDetailsDTO>> GetPharmacyStockByActiveIngrediante(int pharmacyID, string activeIngrediante, int pageNumber, int pageSize);

        /// <summary>
        /// Retrieves pharmacy stock filtered by drug category with pagination.
        /// </summary>
        /// <param name="pharmacyID">The ID of the pharmacy to retrieve stock for</param>
        /// <param name="q">Search by Name or Category or Active ingrediante</param>
        /// <param name="pageNumber">Page number for pagination (must be non-negative)</param>
        /// <param name="pageSize">Number of items per page (must be non-negative, max 100)</param>
        /// <returns>A ServiceResult containing a list of PharmacyProductDetailsDTO objects filtered by category</returns>
        ServiceResult<List<PharmacyProductDetailsDTO>> SearchByNameOrCategoryOrActiveingrediante(ClaimsPrincipal user, int? pharmacyID, string q, int pageNumber, int pageSize);



        /// <summary>
        /// Retrieves detailed information for a specific product in a pharmacy's inventory.
        /// </summary>
        /// <param name="pharmacyId">The ID of the pharmacy (must be positive)</param>
        /// <param name="drugId">The ID of the drug/product (must be positive)</param>
        /// <returns>A ServiceResult containing PharmacyProductDetailsDTO with detailed product information</returns>
        ServiceResult<PharmacyProductDetailsDTO> GetPharmacyProductDetails(int pharmacyId, int drugId);

        /// <summary>
        /// Updates the price of a specific product in a pharmacy's inventory.
        /// Requires appropriate authorization (PharmacyAdmin policy).
        /// </summary>
        /// <param name="user">ClaimsPrincipal containing user authorization information</param>
        /// <param name="drugId">The ID of the drug/product to update (must be positive)</param>
        /// <param name="newPrice">The new price for the product (must be positive)</param>
        /// <param name="pharmacyId">Optional pharmacy ID (required for admin users, ignored for pharmacy users)</param>
        /// <returns>A ServiceResult indicating success or failure of the price update operation</returns>
        ServiceResult<bool> UpdatePharmacyProductPrice(ClaimsPrincipal user, int drugId, decimal newPrice, int? pharmacyId);

        /// <summary>
        /// Increases the quantity of a specific product in a pharmacy's inventory.
        /// Requires appropriate authorization (PharmacyAdmin policy).
        /// </summary>
        /// <param name="user">ClaimsPrincipal containing user authorization information</param>
        /// <param name="drugId">The ID of the drug/product to update (must be positive)</param>
        /// <param name="quantityToIncrease">The amount to increase the quantity by (must be positive)</param>
        /// <param name="pharmacyId">Optional pharmacy ID (required for admin users, ignored for pharmacy users)</param>
        /// <returns>A ServiceResult indicating success or failure of the quantity increase operation</returns>
        ServiceResult<bool> IncreasePharmacyProductQuantity(ClaimsPrincipal user, int drugId, int quantityToIncrease, int? pharmacyId);

        /// <summary>
        /// Decreases the quantity of a specific product in a pharmacy's inventory.
        /// Requires appropriate authorization (PharmacyAdmin policy).
        /// Prevents quantity from going below zero.
        /// </summary>
        /// <param name="user">ClaimsPrincipal containing user authorization information</param>
        /// <param name="drugId">The ID of the drug/product to update (must be positive)</param>
        /// <param name="quantityToDecrease">The amount to decrease the quantity by (must be positive)</param>
        /// <param name="pharmacyId">Optional pharmacy ID (required for admin users, ignored for pharmacy users)</param>
        /// <returns>A ServiceResult indicating success or failure of the quantity decrease operation</returns>
        ServiceResult<bool> DecreasePharmacyProductQuantity(ClaimsPrincipal user, int drugId, int quantityToDecrease, int? pharmacyId);

        /// <summary>
        /// Retrieves all pharmacies that have a specific drug available in their stock inventory.
        /// Designed for patients to find where they can purchase a particular medication.
        /// </summary>
        /// <param name="drugId">The ID of the drug to search for (must be positive)</param>
        /// <returns>A ServiceResult containing a list of PharmacyDTO objects representing pharmacies that stock the drug</returns>
        /// <remarks>
        /// This method is typically used by patients to locate pharmacies where a specific medication is available.
        /// It returns basic pharmacy information including name, address, contact details, and operating hours.
        /// </remarks>
        ServiceResult<List<PharmacyDTO>> getPharmaciesThatHaveDrug(int drugId);
        Task<List<PharmacyWithDistanceDTO>> GetNearest(double lat, double lng, int drugID, int maxResults = 5);
    }
}