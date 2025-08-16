using PharmaLink_API.Models;

namespace PharmaLink_API.Repository.Interfaces
{
    public interface IDrugRepository : IRepository<Drug>
    {
        public Task EditDrug(int id, Drug drug);
        public Task<List<Drug>> GetBatchDrugs(int pageNumber);
        public List<Drug> GetDrugsByFilter(string filter, int size);
    }
}
