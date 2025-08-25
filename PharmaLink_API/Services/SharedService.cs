using PharmaLink_API.Models;
using PharmaLink_API.Models.DTO.SharedDTO;
using PharmaLink_API.Repository.Interfaces;

namespace PharmaLink_API.Services.Interfaces
{
    public class SharedService : ISharedService
    {
        private readonly IPharmacyRepository pharmacyRepository;
        private readonly IDrugRepository drugRepository;

        public SharedService(IPharmacyRepository pharmacyRepository , IDrugRepository drugRepository)
        {
            this.pharmacyRepository = pharmacyRepository;
            this.drugRepository = drugRepository;
        }

        // Method to get pharmacies and drugs by filter with size
        public DrugsAndPharamciesDTO? GetPharmaciesAndDrugsByFilter(string filter, int size = 4)
        {
                 
          
          var pharmacies = pharmacyRepository.GetPharmaciesByFilter(filter, size);
          var drugs = drugRepository.GetDrugsByFilter(filter, size);

         if(pharmacies is null &&  drugs is null)
            {
                return null;
            }

            var drugsAndPharmacies = new DrugsAndPharamciesDTO()
            {
                pharmacies = pharmacies!.Select(d => new PharmacySearchDTO() { Id = d.PharmacyID, Name = d.Name }).ToList(),
                drugs = drugs.Select(d => new DrugSearchDTO() { Id = d.DrugID , Name = d.CommonName }).ToList(),
            };
           return drugsAndPharmacies;
           
        }

    }
}
