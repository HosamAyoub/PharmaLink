using PharmaLink_API.Models;

namespace PharmaLink_API.Repository.Interfaces
{
    public interface IDrugRepository : IRepository<Drug>
    {
        public Task EditDrug( Drug drug);
        public Task<List<Drug>> GetBatchDrugs(int pageNumber);
    }
}
