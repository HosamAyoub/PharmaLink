using PharmaLink_API.Models;

namespace PharmaLink_API.Repository.IRepository
{
    public interface IDrugRepository : IRepository<Drug>
    {
        public Task EditDrug(int id, Drug drug);
        public Task<List<Drug>> GetBatchDrugs(int pageNumber);
    }
}
