using PharmaLink_API.Models;
using PharmaLink_API.Models.DTO.PharmacyStockDTO;
using PharmaLink_API.Core.Results;
using System.Security.Claims;

namespace PharmaLink_API.Services.Interfaces
{
    public interface IPharmacyStockService
    {
        ServiceResult<List<PharmacyProductDetailsDTO>> GetPharmacyStock(int pharmacyId, int pageNumber, int pageSize);
        ServiceResult<bool> AddProductsToPharmacyStock(ClaimsPrincipal user, PharmacyStockDTO pharmacyStockDTO, int? pharmacyId);
        ServiceResult<bool> UpdatePharmacyProduct(ClaimsPrincipal user, pharmacyProductDTO pharmacyProductDTO, int? pharmacyId);
        ServiceResult<bool> DeletePharmacyProduct(ClaimsPrincipal user, int productId, int? pharmacyId);
        public ServiceResult<List<PharmacyProductDetailsDTO>> GetPharmacyStockByCategory(int pharmacyID ,string category, int pageNumber, int pageSize);
    }
}