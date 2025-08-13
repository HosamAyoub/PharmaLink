using PharmaLink_API.Models;
using PharmaLink_API.Repository.Interfaces;

namespace PharmaLink_API.Repository.IRepository
{
    public interface IFavoriteRepository : IRepository<PatientFavoriteDrug>
    {
        Task RemoveRange(IEnumerable<PatientFavoriteDrug> favoriteDrugs);
        Task AddRangeAsync(IEnumerable<PatientFavoriteDrug> favoriteDrugs);
    }
}
