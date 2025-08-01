using System.Linq.Expressions;

namespace PharmaLink_API.Repository.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<T> GetAsync(Expression<Func<T, bool>> filter = null, bool tracking = true, params Expression<Func<T, object>>[] includeProperties);
        Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, params Expression<Func<T, object>>[] includeProperties);
        Task CreateAsync(T entity);
        Task CreateAndSaveAsync(T entity);
        Task RemoveAsync(T entity);
        Task UpdateAsync(T entity);
        Task SaveAsync();
    }
}
