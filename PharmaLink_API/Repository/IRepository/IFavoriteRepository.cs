using PharmaLink_API.Models;

namespace PharmaLink_API.Repository.IRepository
{
    public interface IFavoriteRepository : IRepository<PatientFavoriteDrug>
    {
        Task RemoveRange(IEnumerable<PatientFavoriteDrug> favoriteDrugs);
    }
}
