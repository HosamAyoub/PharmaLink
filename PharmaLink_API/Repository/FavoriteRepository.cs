using PharmaLink_API.Data;
using PharmaLink_API.Models;
using PharmaLink_API.Repository.IRepository;

namespace PharmaLink_API.Repository
{
    public class FavoriteRepository : Repository<PatientFavoriteDrug>, IFavoriteRepository
    {
        private readonly ApplicationDbContext _db;
        public FavoriteRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task RemoveRange(IEnumerable<PatientFavoriteDrug> favoriteDrugs)
        {
            if (favoriteDrugs == null || !favoriteDrugs.Any())
                return;
            _db.PatientFavoriteDrugs.RemoveRange(favoriteDrugs);
            await _db.SaveChangesAsync();
        }

        public async Task AddRangeAsync(IEnumerable<PatientFavoriteDrug> favoriteDrugs)
        {
            if (favoriteDrugs == null || !favoriteDrugs.Any())
                return;
            await _db.PatientFavoriteDrugs.AddRangeAsync(favoriteDrugs);
            await _db.SaveChangesAsync();
        }
    }
}
