using PharmaLink_API.Models.DTO.SharedDTO;

namespace PharmaLink_API.Services.Interfaces
{
    public interface ISharedService
    {
        DrugsAndPharamciesDTO? GetPharmaciesAndDrugsByFilter(string filter, int size = 4);
    }
}
