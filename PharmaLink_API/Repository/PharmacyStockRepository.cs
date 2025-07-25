using Microsoft.EntityFrameworkCore;
using PharmaLink_API.Data;
using PharmaLink_API.Models;
using PharmaLink_API.Repository.Interfaces;
using Microsoft.Extensions.Logging;

namespace PharmaLink_API.Repository
{
    /// <summary>
    /// Implementation of pharmacy stock data access operations using Entity Framework Core
    /// with comprehensive logging, error handling, and business rule enforcement.
    /// </summary>
    /// <remarks>
    /// This repository provides data access for pharmacy stock management including:
    /// - CRUD operations with Entity Framework change tracking
    /// - Eager loading of related entities (Drug and Pharmacy)
    /// - Pagination support for large datasets
    /// - Business rule validation for stock operations
    /// - Comprehensive error handling and logging
    /// - Database constraint and concurrency management
    /// </remarks>
    public class PharmacyStockRepository : Repository<PharmacyProduct>, IPharmacyStockRepository
    {
        private readonly ApplicationDbContext db;
        private readonly ILogger<PharmacyStockRepository> _logger;

        /// <summary>
        /// Initializes a new instance of the PharmacyStockRepository class.
        /// </summary>
        /// <param name="db">The database context for Entity Framework operations</param>
        /// <param name="logger">Logger instance for operation tracking and error logging</param>
        public PharmacyStockRepository(ApplicationDbContext db, ILogger<PharmacyStockRepository> logger) : base(db)
        {
            this.db = db;
            _logger = logger;
        }

        /// <inheritdoc />
        /// <remarks>
        /// Eagerly loads Drug and Pharmacy navigation properties. 
        /// Uses Entity Framework Skip/Take for efficient pagination.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown when database operation fails</exception>
        public IEnumerable<PharmacyProduct> GetPharmacyStockByPharmacyID(int pharmacyId, int pageNumber, int pageSize)
        {
            try
            {
                _logger.LogInformation("Getting pharmacy stock for pharmacy {PharmacyId}, page {PageNumber}, size {PageSize}",
                    pharmacyId, pageNumber, pageSize);

                var pharmacyStock = db.PharmacyStock
                    .Where(ps => ps.PharmacyId == pharmacyId)
                    .Include(ps => ps.Drug)
                    .Include(ps => ps.Pharmacy);

                if (pageNumber > 0 && pageSize > 0)
                {
                    return pharmacyStock.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                }
                else
                {
                    return pharmacyStock.ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting pharmacy stock for pharmacy {PharmacyId}", pharmacyId);
                throw new InvalidOperationException($"Failed to retrieve pharmacy stock for pharmacy {pharmacyId}.", ex);
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// Eagerly loads Drug and Pharmacy navigation properties for all pharmacies.
        /// Uses Entity Framework Skip/Take for efficient pagination.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown when database operation fails</exception>
        public List<PharmacyProduct> GetPharmacyStock(int pageNumber, int pageSize)
        {
            try
            {
                _logger.LogInformation("Getting pharmacy stock, page {PageNumber}, size {PageSize}", pageNumber, pageSize);
                var pharmacyStock = db.PharmacyStock
                    .Include(ps => ps.Drug)
                    .Include(ps => ps.Pharmacy);
                if (pageNumber > 0 && pageSize > 0)
                {
                    return pharmacyStock.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                }
                else
                {
                    return pharmacyStock.ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting pharmacy stock");
                throw new InvalidOperationException("Failed to retrieve pharmacy stock.", ex);
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// Uses AsNoTracking for read-only operations to improve performance.
        /// No navigation properties are loaded for lightweight queries.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown when database operation fails</exception>
        public PharmacyProduct? GetPharmacyProduct(int pharmacyId, int drugId)
        {
            try
            {
                _logger.LogInformation("Getting pharmacy product for pharmacy {PharmacyId} and drug {DrugId}", pharmacyId, drugId);

                return db.PharmacyStock.AsNoTracking()
                    .FirstOrDefault(ps => ps.PharmacyId == pharmacyId && ps.DrugId == drugId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting pharmacy product for pharmacy {PharmacyId} and drug {DrugId}",
                    pharmacyId, drugId);
                throw new InvalidOperationException($"Failed to retrieve pharmacy product for pharmacy {pharmacyId} and drug {drugId}.", ex);
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// Performs duplicate checking within the request and against existing database records.
        /// Uses AddRange for efficient bulk insertion and single SaveChanges call.
        /// </remarks>
        /// <exception cref="ArgumentException">Thrown when products list is null, empty, or contains duplicates</exception>
        /// <exception cref="InvalidOperationException">Thrown when products already exist or database operation fails</exception>
        /// <exception cref="DbUpdateException">Thrown when database constraints are violated</exception>
        public void AddProductsToPharmacyStock(List<PharmacyProduct> pharmacyStock)
        {
            try
            {
                _logger.LogInformation("Adding {Count} products to pharmacy stock", pharmacyStock.Count);

                if (pharmacyStock == null || !pharmacyStock.Any())
                {
                    throw new ArgumentException("Pharmacy stock list cannot be null or empty.");
                }

                // Check for duplicate products in the same request
                var duplicates = pharmacyStock.GroupBy(p => new { p.PharmacyId, p.DrugId })
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicates.Any())
                {
                    throw new ArgumentException($"Duplicate products found in request: {string.Join(", ", duplicates.Select(d => $"Pharmacy {d.PharmacyId}, Drug {d.DrugId}"))}");
                }

                // Check for existing products
                foreach (var product in pharmacyStock)
                {
                    var existing = db.PharmacyStock.AsNoTracking()
                        .FirstOrDefault(ps => ps.PharmacyId == product.PharmacyId && ps.DrugId == product.DrugId);

                    if (existing != null)
                    {
                        throw new InvalidOperationException($"Product with Pharmacy ID {product.PharmacyId} and Drug ID {product.DrugId} already exists.");
                    }
                }

                db.PharmacyStock.AddRange(pharmacyStock);
                db.SaveChanges();

                _logger.LogInformation("Successfully added {Count} products to pharmacy stock", pharmacyStock.Count);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error occurred while adding products to pharmacy stock");
                throw new InvalidOperationException("Failed to add products to pharmacy stock due to database constraints.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding products to pharmacy stock");
                throw new InvalidOperationException("Failed to add products to pharmacy stock.", ex);
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// Verifies product existence before updating using AsNoTracking query.
        /// Uses Entity Framework Update method for tracking and change detection.
        /// Handles concurrency conflicts and database constraint violations.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when the pharmacy product is null</exception>
        /// <exception cref="InvalidOperationException">Thrown when product not found or database operation fails</exception>
        /// <exception cref="DbUpdateConcurrencyException">Thrown when concurrency conflict occurs</exception>
        /// <exception cref="DbUpdateException">Thrown when database constraints are violated</exception>
        public void UpdatePharmacyProduct(PharmacyProduct pharmacyStock)
        {
            try
            {
                _logger.LogInformation("Updating pharmacy product for pharmacy {PharmacyId} and drug {DrugId}",
                    pharmacyStock.PharmacyId, pharmacyStock.DrugId);

                if (pharmacyStock == null)
                {
                    throw new ArgumentNullException(nameof(pharmacyStock), "Pharmacy product cannot be null.");
                }

                var existingProduct = db.PharmacyStock.AsNoTracking()
                    .FirstOrDefault(ps => ps.PharmacyId == pharmacyStock.PharmacyId && ps.DrugId == pharmacyStock.DrugId);

                if (existingProduct == null)
                {
                    throw new InvalidOperationException($"Product with Pharmacy ID {pharmacyStock.PharmacyId} and Drug ID {pharmacyStock.DrugId} not found.");
                }

                db.PharmacyStock.Update(pharmacyStock);
                db.SaveChanges();

                _logger.LogInformation("Successfully updated pharmacy product for pharmacy {PharmacyId} and drug {DrugId}",
                    pharmacyStock.PharmacyId, pharmacyStock.DrugId);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error occurred while updating pharmacy product");
                throw new InvalidOperationException("The product was modified by another user. Please refresh and try again.", ex);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error occurred while updating pharmacy product");
                throw new InvalidOperationException("Failed to update pharmacy product due to database constraints.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating pharmacy product for pharmacy {PharmacyId} and drug {DrugId}",
                    pharmacyStock.PharmacyId, pharmacyStock.DrugId);
                throw new InvalidOperationException("Failed to update pharmacy product.", ex);
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// Validates business rules before deletion (checks for cart items).
        /// Uses Entity Framework Remove method and handles referential integrity.
        /// Order details check is commented out but can be enabled as needed.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when the pharmacy product is null</exception>
        /// <exception cref="InvalidOperationException">Thrown when product has active cart items or database operation fails</exception>
        /// <exception cref="DbUpdateException">Thrown when database constraints are violated</exception>
        public void DeletePharmacyProduct(PharmacyProduct pharmacyStock)
        {
            try
            {
                _logger.LogInformation("Deleting pharmacy product for pharmacy {PharmacyId} and drug {DrugId}",
                    pharmacyStock.PharmacyId, pharmacyStock.DrugId);

                if (pharmacyStock == null)
                {
                    throw new ArgumentNullException(nameof(pharmacyStock), "Pharmacy product cannot be null.");
                }

                // Check if product has active orders or cart items
                //var hasActiveOrders = db.OrderDetails.Any(od => od.PharmacyId == pharmacyStock.PharmacyId
                //    && od.DrugId == pharmacyStock.DrugId);

                //if (hasActiveOrders)
                //{
                //    throw new InvalidOperationException("Cannot delete product that has active orders.");
                //}

                var hasCartItems = db.CartItems.Any(ci => ci.PharmacyId == pharmacyStock.PharmacyId
                    && ci.DrugId == pharmacyStock.DrugId);

                if (hasCartItems)
                {
                    throw new InvalidOperationException("Cannot delete product that is in customer carts.");
                }

                db.PharmacyStock.Remove(pharmacyStock);
                db.SaveChanges();

                _logger.LogInformation("Successfully deleted pharmacy product for pharmacy {PharmacyId} and drug {DrugId}",
                    pharmacyStock.PharmacyId, pharmacyStock.DrugId);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error occurred while deleting pharmacy product");
                throw new InvalidOperationException("Failed to delete pharmacy product due to database constraints.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting pharmacy product for pharmacy {PharmacyId} and drug {DrugId}",
                    pharmacyStock.PharmacyId, pharmacyStock.DrugId);
                throw new InvalidOperationException("Failed to delete pharmacy product.", ex);
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// Filters by drug category using navigation property Drug.Category.
        /// Eagerly loads Drug and Pharmacy navigation properties.
        /// Uses Entity Framework Skip/Take for efficient pagination.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown when database operation fails</exception>
        public List<PharmacyProduct> getPharmacyStockByCategory(int pharmacyId, string category, int pageNumber, int pageSize)
        {
            try
            {
                _logger.LogInformation("Getting pharmacy stock by category {Category}, page {PageNumber}, size {PageSize}",
                    category, pageNumber, pageSize);
                var pharmacyStock = db.PharmacyStock
                    .Where(ps => ps.Drug!.Category == category && ps.PharmacyId == pharmacyId)
                    .Include(ps => ps.Drug)
                    .Include(ps => ps.Pharmacy);

                if (pageNumber > 0 && pageSize > 0)
                {
                    return pharmacyStock.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                }
                else
                {
                    return pharmacyStock.ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting pharmacy stock by category {Category}", category);
                throw new InvalidOperationException($"Failed to retrieve pharmacy stock for category {category}.", ex);
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// Uses change tracking for atomic price updates.
        /// Verifies product existence before updating price field.
        /// Handles concurrency conflicts and database constraint violations.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown when product not found or database operation fails</exception>
        /// <exception cref="DbUpdateConcurrencyException">Thrown when concurrency conflict occurs</exception>
        /// <exception cref="DbUpdateException">Thrown when database constraints are violated</exception>
        public void UpdatePharmacyProductPrice(int pharmacyId, int drugId, decimal newPrice)
        {
            try
            {
                _logger.LogInformation("Updating price for pharmacy {PharmacyId} and drug {DrugId} to {NewPrice}",
                    pharmacyId, drugId, newPrice);

                var existingProduct = db.PharmacyStock
                    .FirstOrDefault(ps => ps.PharmacyId == pharmacyId && ps.DrugId == drugId);

                if (existingProduct == null)
                {
                    throw new InvalidOperationException($"Product with Pharmacy ID {pharmacyId} and Drug ID {drugId} not found.");
                }

                existingProduct.Price = newPrice;
                db.SaveChanges();

                _logger.LogInformation("Successfully updated price for pharmacy {PharmacyId} and drug {DrugId} to {NewPrice}",
                    pharmacyId, drugId, newPrice);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error occurred while updating product price");
                throw new InvalidOperationException("The product was modified by another user. Please refresh and try again.", ex);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error occurred while updating product price");
                throw new InvalidOperationException("Failed to update product price due to database constraints.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating price for pharmacy {PharmacyId} and drug {DrugId}",
                    pharmacyId, drugId);
                throw new InvalidOperationException("Failed to update product price.", ex);
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// Eagerly loads Drug and Pharmacy navigation properties for detailed information.
        /// Uses change tracking for complete entity relationships.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown when database operation fails</exception>
        public PharmacyProduct? GetPharmacyProductWithDetails(int pharmacyId, int drugId)
        {
            try
            {
                _logger.LogInformation("Getting detailed pharmacy product for pharmacy {PharmacyId} and drug {DrugId}", pharmacyId, drugId);

                return db.PharmacyStock
                    .Include(ps => ps.Drug)
                    .Include(ps => ps.Pharmacy)
                    .FirstOrDefault(ps => ps.PharmacyId == pharmacyId && ps.DrugId == drugId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting detailed pharmacy product for pharmacy {PharmacyId} and drug {DrugId}",
                    pharmacyId, drugId);
                throw new InvalidOperationException($"Failed to retrieve detailed pharmacy product for pharmacy {pharmacyId} and drug {drugId}.", ex);
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// Uses change tracking for atomic quantity updates.
        /// Performs addition operation and logs both operation and final quantity.
        /// Handles concurrency conflicts and database constraint violations.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown when product not found or database operation fails</exception>
        /// <exception cref="DbUpdateConcurrencyException">Thrown when concurrency conflict occurs</exception>
        /// <exception cref="DbUpdateException">Thrown when database constraints are violated</exception>
        public void IncreasePharmacyProductQuantity(int pharmacyId, int drugId, int quantityToIncrease)
        {
            try
            {
                _logger.LogInformation("Increasing quantity for pharmacy {PharmacyId} and drug {DrugId} by {QuantityToIncrease}",
                    pharmacyId, drugId, quantityToIncrease);

                var existingProduct = db.PharmacyStock
                    .FirstOrDefault(ps => ps.PharmacyId == pharmacyId && ps.DrugId == drugId);

                if (existingProduct == null)
                {
                    throw new InvalidOperationException($"Product with Pharmacy ID {pharmacyId} and Drug ID {drugId} not found.");
                }

                existingProduct.QuantityAvailable += quantityToIncrease;
                db.SaveChanges();

                _logger.LogInformation("Successfully increased quantity for pharmacy {PharmacyId} and drug {DrugId} by {QuantityToIncrease}. New quantity: {NewQuantity}",
                    pharmacyId, drugId, quantityToIncrease, existingProduct.QuantityAvailable);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error occurred while increasing product quantity");
                throw new InvalidOperationException("The product was modified by another user. Please refresh and try again.", ex);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error occurred while increasing product quantity");
                throw new InvalidOperationException("Failed to increase product quantity due to database constraints.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while increasing quantity for pharmacy {PharmacyId} and drug {DrugId}",
                    pharmacyId, drugId);
                throw new InvalidOperationException("Failed to increase product quantity.", ex);
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// Validates sufficient quantity is available before performing decrease operation.
        /// Uses change tracking for atomic quantity updates and prevents negative quantities.
        /// Logs both operation and final quantity for audit purposes.
        /// Handles concurrency conflicts and database constraint violations.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown when product not found, insufficient quantity, or database operation fails</exception>
        /// <exception cref="DbUpdateConcurrencyException">Thrown when concurrency conflict occurs</exception>
        /// <exception cref="DbUpdateException">Thrown when database constraints are violated</exception>
        public void DecreasePharmacyProductQuantity(int pharmacyId, int drugId, int quantityToDecrease)
        {
            try
            {
                _logger.LogInformation("Decreasing quantity for pharmacy {PharmacyId} and drug {DrugId} by {QuantityToDecrease}",
                    pharmacyId, drugId, quantityToDecrease);

                var existingProduct = db.PharmacyStock
                    .FirstOrDefault(ps => ps.PharmacyId == pharmacyId && ps.DrugId == drugId);

                if (existingProduct == null)
                {
                    throw new InvalidOperationException($"Product with Pharmacy ID {pharmacyId} and Drug ID {drugId} not found.");
                }

                if (existingProduct.QuantityAvailable < quantityToDecrease)
                {
                    throw new InvalidOperationException($"Cannot decrease quantity by {quantityToDecrease}. Current quantity is {existingProduct.QuantityAvailable}.");
                }

                existingProduct.QuantityAvailable -= quantityToDecrease;
                db.SaveChanges();

                _logger.LogInformation("Successfully decreased quantity for pharmacy {PharmacyId} and drug {DrugId} by {QuantityToDecrease}. New quantity: {NewQuantity}",
                    pharmacyId, drugId, quantityToDecrease, existingProduct.QuantityAvailable);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error occurred while decreasing product quantity");
                throw new InvalidOperationException("The product was modified by another user. Please refresh and try again.", ex);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error occurred while decreasing product quantity");
                throw new InvalidOperationException("Failed to decrease product quantity due to database constraints.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while decreasing quantity for pharmacy {PharmacyId} and drug {DrugId}",
                    pharmacyId, drugId);
                throw new InvalidOperationException("Failed to decrease product quantity.", ex);
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// Uses LINQ join operation to efficiently find pharmacies with the specified drug.
        /// Only returns pharmacies that actually have the drug in stock (quantity > 0).
        /// Uses AsNoTracking for optimal read-only performance.
        /// Returns distinct pharmacies to avoid duplicates if a pharmacy has multiple entries for the same drug.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown when database operation fails</exception>
        public List<Pharmacy>? getPharmaciesThatHaveDrug(int drugId)
        {
            try
            {
                _logger.LogInformation("Getting pharmacies that have drug with ID {DrugId}", drugId);

                var pharmacies = db.PharmacyStock
                    .AsNoTracking()
                    .Where(ps => ps.DrugId == drugId && ps.QuantityAvailable > 0)
                    .Include(ps => ps.Pharmacy)
                    .Select(ps => ps.Pharmacy)
                    .Distinct()
                    .ToList();

                _logger.LogInformation("Found {Count} pharmacies that have drug with ID {DrugId}", 
                    pharmacies?.Count ?? 0, drugId);

                return pharmacies;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting pharmacies that have drug with ID {DrugId}", drugId);
                throw new InvalidOperationException($"Failed to retrieve pharmacies that have drug with ID {drugId}.", ex);
            }
        }
    }
}
