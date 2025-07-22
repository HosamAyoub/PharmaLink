using Microsoft.EntityFrameworkCore;
using PharmaLink_API.Data;
using PharmaLink_API.Models;
using PharmaLink_API.Repository.Interfaces;
using Microsoft.Extensions.Logging;

namespace PharmaLink_API.Repository
{
    public class PharmacyStockRepository : Repository<PharmacyProduct>, IPharmacyStockRepository
    {
        private readonly ApplicationDbContext db;
        private readonly ILogger<PharmacyStockRepository> _logger;

        public PharmacyStockRepository(ApplicationDbContext db, ILogger<PharmacyStockRepository> logger) : base(db)
        {
            this.db = db;
            _logger = logger;
        }

        public IEnumerable<PharmacyProduct> GetPharmacyStock(int pharmacyId, int pageNumber, int pageSize)
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

                if (pharmacyStock.Price < 0)
                {
                    throw new ArgumentException("Product price cannot be negative.");
                }

                if (pharmacyStock.QuantityAvailable < 0)
                {
                    throw new ArgumentException("Product quantity cannot be negative.");
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
                var hasActiveOrders = db.OrderDetails.Any(od => od.PharmacyId == pharmacyStock.PharmacyId 
                    && od.DrugId == pharmacyStock.DrugId);
                
                if (hasActiveOrders)
                {
                    throw new InvalidOperationException("Cannot delete product that has active orders.");
                }

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
    }
}
