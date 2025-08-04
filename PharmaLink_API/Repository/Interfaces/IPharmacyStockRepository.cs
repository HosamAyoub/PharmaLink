using PharmaLink_API.Models;
using PharmaLink_API.Models.DTO.PharmacyStockDTO;

namespace PharmaLink_API.Repository.Interfaces
{
    /// <summary>
    /// Defines the contract for pharmacy stock data access operations including CRUD operations,
    /// inventory management, and specialized query methods.
    /// </summary>
    /// <remarks>
    /// This interface extends the base IRepository interface to provide specialized methods for:
    /// - Pharmacy stock retrieval with pagination and filtering
    /// - Product inventory management operations
    /// - Price and quantity management
    /// - Category-based stock queries
    /// - Entity relationship handling with Drug and Pharmacy entities
    /// </remarks>
    public interface IPharmacyStockRepository : IRepository<PharmacyProduct>
    {

        /// <summary>
        /// Retrieves pharmacy stock for a specific pharmacy and related entity data.
        /// </summary>
        /// <param name="pharmacyId">The ID of the pharmacy to retrieve stock for</param>
        /// <returns>An enumerable collection of PharmacyProduct entities with Drug and Pharmacy navigation properties loaded</returns>
        IEnumerable<PharmacyProduct> GetAllPharmacyStockByPharmacyID(int pharmacyId);


        /// <summary>
        /// Retrieves pharmacy stock for a specific pharmacy with pagination and related entity data.
        /// </summary>
        /// <param name="pharmacyId">The ID of the pharmacy to retrieve stock for</param>
        /// <param name="pageNumber">Page number for pagination (0 or negative returns all records)</param>
        /// <param name="pageSize">Number of items per page (0 or negative returns all records)</param>
        /// <returns>An enumerable collection of PharmacyProduct entities with Drug and Pharmacy navigation properties loaded</returns>
        IEnumerable<PharmacyProduct> GetPharmacyStockByPharmacyID(int pharmacyId, int pageNumber, int pageSize , out int totalSize);

        /// <summary>
        /// Retrieves all pharmacy stock across all pharmacies with pagination and related entity data.
        /// </summary>
        /// <param name="pageNumber">Page number for pagination (0 or negative returns all records)</param>
        /// <param name="pageSize">Number of items per page (0 or negative returns all records)</param>
        /// <returns>A list of PharmacyProduct entities with Drug and Pharmacy navigation properties loaded</returns>
        List<PharmacyProduct> GetPharmacyStock(int pageNumber, int pageSize);

        /// <summary>
        /// Adds multiple products to pharmacy stock inventory with duplicate checking and validation.
        /// </summary>
        /// <param name="PharmacyProduct">List of PharmacyProduct entities to add to the stock</param>
        /// <exception cref="ArgumentException">Thrown when products list is null, empty, or contains duplicates</exception>
        /// <exception cref="InvalidOperationException">Thrown when products already exist in the database</exception>
        void AddProductsToPharmacyStock(List<PharmacyProduct> PharmacyProduct);

        /// <summary>
        /// Updates an existing pharmacy product in the inventory.
        /// </summary>
        /// <param name="PharmacyProduct">The PharmacyProduct entity with updated information</param>
        /// <exception cref="ArgumentNullException">Thrown when the pharmacy product is null</exception>
        /// <exception cref="InvalidOperationException">Thrown when the product is not found</exception>
        void UpdatePharmacyProduct(PharmacyProduct PharmacyProduct);

        /// <summary>
        /// Retrieves a specific pharmacy product without change tracking for read-only operations.
        /// </summary>
        /// <param name="pharmacyId">The ID of the pharmacy</param>
        /// <param name="drugId">The ID of the drug/product</param>
        /// <returns>The PharmacyProduct entity if found, null otherwise</returns>
        PharmacyProduct? GetPharmacyProduct(int pharmacyId, int drugId);

        /// <summary>
        /// Deletes a pharmacy product from inventory with business rule validation.
        /// </summary>
        /// <param name="PharmacyProduct">The PharmacyProduct entity to delete</param>
        /// <exception cref="ArgumentNullException">Thrown when the pharmacy product is null</exception>
        /// <exception cref="InvalidOperationException">Thrown when product has active cart items or cannot be deleted</exception>
        void DeletePharmacyProduct(PharmacyProduct PharmacyProduct);

        /// <summary>
        /// Retrieves pharmacy stock filtered by drug category with pagination and related entity data.
        /// </summary>
        /// <param name="pharamcyId">The ID of the pharmacy to retrieve stock for</param>
        /// <param name="category">The drug category to filter by</param>
        /// <param name="pageNumber">Page number for pagination (0 or negative returns all records)</param>
        /// <param name="pageSize">Number of items per page (0 or negative returns all records)</param>
        /// <returns>A list of PharmacyProduct entities matching the category filter with Drug and Pharmacy navigation properties loaded</returns>
        List<PharmacyProduct> getPharmacyStockByCategory(int pharamcyId, string category, int pageNumber, int pageSize);


        /// <summary>
        /// Retrieves pharmacy stock filtered by drug category with pagination and related entity data.
        /// </summary>
        /// <param name="pharamcyId">The ID of the pharmacy to retrieve stock for</param>
        /// <param name="drugName">The drug drug Name to filter by</param>
        /// <param name="pageNumber">Page number for pagination (0 or negative returns all records)</param>
        /// <param name="pageSize">Number of items per page (0 or negative returns all records)</param>
        /// <returns>A list of PharmacyProduct entities matching the category filter with Drug and Pharmacy navigation properties loaded</returns>
        List<PharmacyProduct> getPharmacyStockByDrugName(int pharamcyId, string drugName, int pageNumber, int pageSize);


        /// <summary>
        /// Retrieves pharmacy stock filtered by drug category with pagination and related entity data.
        /// </summary>
        /// <param name="pharamcyId">The ID of the pharmacy to retrieve stock for</param>
        /// <param name="activeIngrediante">The drug drug Name to filter by</param>
        /// <param name="pageNumber">Page number for pagination (0 or negative returns all records)</param>
        /// <param name="pageSize">Number of items per page (0 or negative returns all records)</param>
        /// <returns>A list of PharmacyProduct entities matching the category filter with Drug and Pharmacy navigation properties loaded</returns>
        List<PharmacyProduct> getPharmacyStockByActiveIngrediante(int pharamcyId, string activeIngrediante, int pageNumber, int pageSize);

        /// <summary>
        /// Retrieves a specific pharmacy product with detailed related entity information.
        /// </summary>
        /// <param name="pharmacyId">The ID of the pharmacy</param>
        /// <param name="drugId">The ID of the drug/product</param>
        /// <returns>The PharmacyProduct entity with Drug and Pharmacy navigation properties loaded if found, null otherwise</returns>
        PharmacyProduct? GetPharmacyProductWithDetails(int pharmacyId, int drugId);

        /// <summary>
        /// Updates only the price of a specific pharmacy product.
        /// </summary>
        /// <param name="pharmacyId">The ID of the pharmacy</param>
        /// <param name="drugId">The ID of the drug/product</param>
        /// <param name="newPrice">The new price to set for the product</param>
        /// <exception cref="InvalidOperationException">Thrown when the product is not found</exception>
        void UpdatePharmacyProductPrice(int pharmacyId, int drugId, decimal newPrice);

        /// <summary>
        /// Increases the quantity of a specific pharmacy product by the specified amount.
        /// </summary>
        /// <param name="pharmacyId">The ID of the pharmacy</param>
        /// <param name="drugId">The ID of the drug/product</param>
        /// <param name="quantityToIncrease">The amount to increase the quantity by</param>
        /// <exception cref="InvalidOperationException">Thrown when the product is not found</exception>
        void IncreasePharmacyProductQuantity(int pharmacyId, int drugId, int quantityToIncrease);

        /// <summary>
        /// Decreases the quantity of a specific pharmacy product by the specified amount.
        /// Prevents quantity from going below zero.
        /// </summary>
        /// <param name="pharmacyId">The ID of the pharmacy</param>
        /// <param name="drugId">The ID of the drug/product</param>
        /// <param name="quantityToDecrease">The amount to decrease the quantity by</param>
        /// <exception cref="InvalidOperationException">Thrown when the product is not found or insufficient quantity available</exception>
        void DecreasePharmacyProductQuantity(int pharmacyId, int drugId, int quantityToDecrease);

        /// <summary>
        /// Retrieves all pharmacies that have a specific drug in their stock inventory.
        /// </summary>
        /// <param name="drugId">The ID of the drug to search for in pharmacy inventories</param>
        /// <returns>A list of Pharmacy entities that have the specified drug in stock, null if no pharmacies found</returns>
        /// <exception cref="InvalidOperationException">Thrown when database operation fails</exception>
        List<Pharmacy>? getPharmaciesThatHaveDrug(int drugId);
    }
}
